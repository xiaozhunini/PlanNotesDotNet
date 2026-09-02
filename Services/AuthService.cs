using AutoMapper;
using Microsoft.Extensions.Options;
using PlanNoteServer.Configuration;
using PlanNoteServer.DTOs;
using PlanNoteServer.DTOs.Auth;
using PlanNoteServer.Models;
using PlanNoteServer.Repositories;
using PlanNoteServer.Services.IServices;

namespace PlanNoteServer.Services
{
    /// <summary>
    /// 认证服务实现类（负责微信登录、用户注册/创建、令牌管理）。
    /// RefreshToken 存储已从 DB 字段迁移到 Redis（见 RedisTokenStore），支持家族级撤销与复用检测。
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IRepository<Users> _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ITokenStore _tokenStore;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;

        /// <summary>
        /// 构造函数，通过依赖注入获取所需的仓储和服务实例
        /// </summary>
        public AuthService(
            IRepository<Users> userRepository,
            IJwtService jwtService,
            ITokenStore tokenStore,
            IMapper mapper,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _tokenStore = tokenStore;
            _mapper = mapper;
            _jwtSettings = jwtSettings.Value;
        }

        /// <summary>
        /// 微信用户注册/创建（基于 openid 直接创建用户记录）
        /// </summary>
        public async Task<UserDto> RegisterAsync(RegisterRequest request)
        {
            // 1. 校验 openid 是否已被注册
            var existing = await _userRepository.FindAsync(u => u.OpenId == request.OpenId);
            if (existing.Any())
            {
                throw new Exception("该 openid 已注册");
            }

            // 2. 构建新用户实体
            var user = new Users
            {
                OpenId = request.OpenId,
                UnionId = request.UnionId,
                NickName = request.NickName ?? string.Empty,
                AvatarUrl = request.AvatarUrl,
                UserStatus = 0       // 默认正常状态
            };

            // 3. 保存到数据库并转换为 DTO 返回
            var createdUser = await _userRepository.AddAsync(user);
            return _mapper.Map<UserDto>(createdUser);
        }

        /// <summary>
        /// 微信登录（通过 wx.login 的 code 换取 openid，用户不存在则自动注册）。
        /// 登录成功后 RefreshToken 写入 Redis（带家族ID），不再写 Users 表。
        /// </summary>
        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            // TODO: 调用微信 jscode2session 接口，用 code 换取 openid + session_key
            // 当前开发阶段直接用 code 作为 openid，生产环境必须替换为真实微信 API 调用
            var openId = request.Code;

            // 1. 根据 openid 查找用户
            var users = await _userRepository.FindAsync(u => u.OpenId == openId);
            var user = users.FirstOrDefault();

            if (user == null)
            {
                // 2. 用户不存在则自动创建（首次登录即注册）
                user = new Users
                {
                    OpenId = openId,
                    NickName = request.NickName ?? string.Empty,
                    AvatarUrl = request.AvatarUrl,
                    UserStatus = 0,
                    LastLoginTime = DateTime.Now
                };
                user = await _userRepository.AddAsync(user);
            }
            else
            {
                // 3. 校验账号状态（0正常/1禁用）
                if (user.UserStatus == 1)
                {
                    throw new Exception("账号已被禁用");
                }

                // 4. 更新最后登录时间
                user.LastLoginTime = DateTime.Now;
                await _userRepository.UpdateAsync(user);
            }

            // 5. 生成 JWT 令牌对（AccessToken + RefreshToken）
            var tokenResponse = _jwtService.GenerateTokens(user);

            // 6. RefreshToken 写入 Redis（新建家族ID，TTL = 配置的刷新令牌过期天数）
            var familyId = Guid.NewGuid();
            var ttl = TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays);
            await _tokenStore.StoreAsync(tokenResponse.RefreshToken, user.Id, familyId, ttl);

            return tokenResponse;
        }

        /// <summary>
        /// 刷新令牌（用 RefreshToken 换取新令牌对）。
        /// 关键安全机制：复用检测 —— 当一条已 used/revoked 的旧 token 被再次提交时，
        /// 视为令牌泄露，立即撤销整个家族所有 token，强制用户重新登录。
        /// </summary>
        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            // 1. 从 Redis 查询该 RefreshToken 的元数据
            var info = await _tokenStore.GetAsync(refreshToken);
            if (info == null)
            {
                throw new Exception("刷新令牌无效或已过期");
            }

            // 2. 复用检测：若 token 已被轮转（used）或撤销（revoked），说明有人使用了旧 token
            if (info.Status == "used" || info.Status == "revoked")
            {
                // 立即作废整个家族的所有 token，强制重新登录
                await _tokenStore.RevokeFamilyAsync(info.FamilyId);
                throw new Exception("检测到令牌复用，所有令牌已作废，请重新登录");
            }

            // 3. 查询用户并校验状态
            var user = await _userRepository.GetByIdAsync(info.UserId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }
            if (user.UserStatus == 1)
            {
                throw new Exception("账号已被禁用");
            }

            // 4. 重新生成一套全新的令牌对
            var tokenResponse = _jwtService.GenerateTokens(user);

            // 5. 标记旧 token 为 used（带 replaced 字段记录新 token 摘要，审计用）
            await _tokenStore.MarkUsedAsync(refreshToken, tokenResponse.RefreshToken);

            // 6. 写入新 token（同 familyId 继承，保持家族链可追溯）
            var ttl = TimeSpan.FromDays(_jwtSettings.RefreshTokenExpirationDays);
            await _tokenStore.StoreAsync(tokenResponse.RefreshToken, user.Id, info.FamilyId, ttl);

            return tokenResponse;
        }

        /// <summary>
        /// 撤销/注销指定的 RefreshToken（通常用于用户主动退出登录）。
        /// 注意：撤销单条 token 不撤销家族 —— 用户在另一设备仍可使用同家族其他 token。
        /// 如需强制全端下线，应使用 RevokeFamilyAsync。
        /// </summary>
        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var info = await _tokenStore.GetAsync(refreshToken);
            if (info == null)
            {
                return false;
            }
            await _tokenStore.RevokeAsync(refreshToken);
            return true;
        }
    }
}
