using PlanNoteServer.Models;

namespace PlanNoteServer.Services.IServices
{
    /// <summary>
    /// 令牌存储抽象（用于解耦 AuthService 与具体存储介质）。
    /// 当前实现：RedisTokenStore（基于 Redis Hash + 反向索引，支持家族级撤销与复用检测）。
    /// 后续如需切换为 DB 或其他存储，仅需替换实现，不改 AuthService。
    /// </summary>
    public interface ITokenStore
    {
        /// <summary>
        /// 存储一条新的 RefreshToken（登录或刷新时调用）。
        /// 写入 Redis Hash：uid / family / status=active / created，并加入家族反向索引。
        /// </summary>
        /// <param name="refreshToken">明文 RefreshToken</param>
        /// <param name="userId">用户ID</param>
        /// <param name="familyId">令牌家族ID（登录时新建 Guid，同一次登录的轮转链共享）</param>
        /// <param name="ttl">过期时间（Redis TTL，自动回收）</param>
        Task StoreAsync(string refreshToken, long userId, Guid familyId, TimeSpan ttl);

        /// <summary>
        /// 查询 RefreshToken 的状态信息。
        /// </summary>
        /// <param name="refreshToken">明文 RefreshToken</param>
        /// <returns>令牌信息（uid/family/status/replaced），不存在返回 null</returns>
        Task<RefreshTokenInfo?> GetAsync(string refreshToken);

        /// <summary>
        /// 标记旧 token 为已使用（轮转），并记录新 token 的摘要，便于审计与家族撤销。
        /// </summary>
        /// <param name="oldToken">被轮转的旧 RefreshToken</param>
        /// <param name="newToken">轮转生成的新 RefreshToken</param>
        Task MarkUsedAsync(string oldToken, string newToken);

        /// <summary>
        /// 撤销单条 RefreshToken（登出场景：用户主动退出，仅令当前 token 失效）。
        /// </summary>
        /// <param name="refreshToken">要撤销的 RefreshToken</param>
        Task RevokeAsync(string refreshToken);

        /// <summary>
        /// 撤销整个家族的 RefreshToken（复用检测场景：旧 token 被再次使用，立即作废该家族所有 token）。
        /// </summary>
        /// <param name="familyId">令牌家族ID</param>
        Task RevokeFamilyAsync(Guid familyId);
    }

    /// <summary>
    /// RefreshToken 在存储中的元数据（用于复用检测和家族撤销）。
    /// </summary>
    public class RefreshTokenInfo
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 令牌家族ID（同一次登录的轮转链共享）
        /// </summary>
        public Guid FamilyId { get; set; }

        /// <summary>
        /// 令牌状态：active(正常) / used(已轮转) / revoked(已撤销)
        /// </summary>
        public string Status { get; set; } = "active";

        /// <summary>
        /// 轮转后的新 token 摘要（可选，审计用）
        /// </summary>
        public string? Replaced { get; set; }
    }
}
