using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PlanNoteServer.Configuration;
using PlanNoteServer.Services.IServices;
using StackExchange.Redis;

namespace PlanNoteServer.Services
{
    /// <summary>
    /// 基于 Redis 的令牌存储实现。
    ///
    /// Key 设计：
    ///   {InstanceName}auth:refresh:{tokenSha256}    Hash(uid / family / status / replaced / created)
    ///   {InstanceName}auth:refresh:family:{familyId} Set(tokenSha256...)，反向索引用于家族级撤销
    ///
    /// TTL：等于 RefreshToken 的有效期（默认 7 天），过期自动回收，无需手工清理。
    /// 复用检测：当一条状态为 used/revoked 的旧 token 被再次提交时，撤销整个家族所有 token。
    /// </summary>
    public class RedisTokenStore : ITokenStore
    {
        private readonly IDatabase _db;
        private readonly RedisSettings _settings;

        public RedisTokenStore(IConnectionMultiplexer mux, IOptions<RedisSettings> settings)
        {
            _settings = settings.Value;
            _db = mux.GetDatabase(_settings.DefaultDatabase);
        }

        public async Task StoreAsync(string refreshToken, long userId, Guid familyId, TimeSpan ttl)
        {
            var tokenSha = Sha256(refreshToken);
            var key = RefreshKey(tokenSha);
            var familyKey = FamilyKey(familyId);

            // 写入 Hash（status=active）
            // 注意：Redis Hash 字段值不允许为 null，新建 token 时 replaced 字段先不写入，
            // 等 MarkUsedAsync 轮转时再写入新 token 的 SHA。GetAsync 用 GetValueOrDefault 读取，缺失即 null。
            await _db.HashSetAsync(key, new HashEntry[]
            {
                new("uid", userId.ToString()),
                new("family", familyId.ToString()),
                new("status", "active"),
                new("created", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            });

            await _db.KeyExpireAsync(key, ttl);

            // 加入家族反向索引（Set），同时刷新 TTL
            await _db.SetAddAsync(familyKey, tokenSha);
            await _db.KeyExpireAsync(familyKey, ttl);
        }

        public async Task<RefreshTokenInfo?> GetAsync(string refreshToken)
        {
            var tokenSha = Sha256(refreshToken);
            var key = RefreshKey(tokenSha);

            var entries = await _db.HashGetAllAsync(key);
            if (entries.Length == 0) return null;

            var map = new Dictionary<string, string?>(entries.Length, StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                var name = (string?)entry.Name;
                if (name != null) map[name] = (string?)entry.Value;
            }

            return new RefreshTokenInfo
            {
                UserId = long.Parse(map.GetValueOrDefault("uid") ?? "0"),
                FamilyId = Guid.Parse(map.GetValueOrDefault("family") ?? Guid.Empty.ToString()),
                Status = map.GetValueOrDefault("status") ?? "active",
                Replaced = map.GetValueOrDefault("replaced")
            };
        }

        public async Task MarkUsedAsync(string oldToken, string newToken)
        {
            var oldSha = Sha256(oldToken);
            var newSha = Sha256(newToken);
            var key = RefreshKey(oldSha);

            // 仅更新这两个字段，不动 TTL（保留原有效期）
            await _db.HashSetAsync(key, new HashEntry[]
            {
                new("status", "used"),
                new("replaced", newSha)
            });
        }

        public async Task RevokeAsync(string refreshToken)
        {
            var tokenSha = Sha256(refreshToken);
            var key = RefreshKey(tokenSha);

            await _db.HashSetAsync(key, new HashEntry[]
            {
                new("status", "revoked")
            });
        }

        public async Task RevokeFamilyAsync(Guid familyId)
        {
            var familyKey = FamilyKey(familyId);

            // 拿到该家族下所有 token 的 SHA
            var members = await _db.SetMembersAsync(familyKey);

            foreach (var member in members)
            {
                var key = RefreshKey((string)member!);
                // 标记整族 revoked
                await _db.HashSetAsync(key, new HashEntry[]
                {
                    new("status", "revoked")
                });
            }

            // 反向索引可以保留到自然过期，不强制删
        }

        // ===== 私有辅助方法 =====

        private string Sha256(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private string RefreshKey(string tokenSha) =>
            $"{_settings.InstanceName}auth:refresh:{tokenSha}";

        private string FamilyKey(Guid familyId) =>
            $"{_settings.InstanceName}auth:refresh:family:{familyId}";
    }
}
