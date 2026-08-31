using AutoMapper;
using PlanNoteServer.DTOs;
using PlanNoteServer.DTOs.Auth;
using PlanNoteServer.Models;
using PlanNoteServer.Repositories;
using PlanNoteServer.Services.IServices;

namespace PlanNoteServer.Services
{
    /// <summary>
    /// 认证服务实现类（负责微信登录、用户注册/创建、令牌管理）
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IRepository<Users> _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;

        /// <summary>
        /// 构造函数，通过依赖注入获取所需的仓储和服务实例
        /// </summary>
        /// <param name="userRepository">用户数据仓储</param>
        /// <param name="jwtService">JWT 令牌生成服务</param>
        /// <param name="mapper">对象映射器</param>
        public AuthService(
            IRepository<Users> userRepository,
            IJwtService jwtService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _mapper = mapper;
        }

        /// <summary>
        /// 微信用户注册/创建（基于 openid 直接创建用户记录）
        /// </summary>
        /// <param name="request">包含 openid 及可选资料的注册请求</param>
        /// <returns>注册成功后返回的用户信息 DTO</returns>
        /// <exception cref="Exception">当 openid 已存在时抛出异常</exception>
        public async Task<UserDto> RegisterAsync(RegisterRequest request)
        {
            // 1. 校验 openid 是否已被注册
            var existing = await _userRepository.FindAsync(u => u.OpenId == request.OpenId);
            if (existing.Any())
            {
                throw new Exception("该 openid 已注册");
            }

            // 2. 构建新用户实体并设置默认属性
            // 角色已迁移到 user_roles 关联表，新建用户后应在业务层为其绑定默认角色
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
        /// 微信登录（通过 wx.login 的 code 换取 openid，用户不存在则自动注册）
        /// </summary>
        /// <param name="request">包含 code 及可选资料的登录请求</param>
        /// <returns>登录成功后返回包含 AccessToken 和 RefreshToken 的响应对象</returns>
        /// <exception cref="Exception">当账号被禁用时抛出异常</exception>
        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            // TODO: 调用微信 jscode2session 接口，用 code 换取 openid + session_key
            //   var (openId, unionId, sessionKey) = await CallWxJsCode2SessionAsync(request.Code);
            // 当前开发阶段直接用 code 作为 openid，生产环境必须替换为真实微信 API 调用
            var openId = request.Code;

            // 1. 根据 openid 查找用户
            var users = await _userRepository.FindAsync(u => u.OpenId == openId);
            var user = users.FirstOrDefault();

            if (user == null)
            {
                // 2. 用户不存在则自动创建（首次登录即注册）
                // 角色已迁移到 user_roles 关联表，新建用户后应绑定默认角色
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

            // 6. 保存 RefreshToken 及其过期时间（用于后续令牌轮转）
            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiryTime = tokenResponse.RefreshTokenExpiresAt;
            await _userRepository.UpdateAsync(user);

            return tokenResponse;
        }

        /// <summary>
        /// 刷新令牌（使用长效的 RefreshToken 换取新的令牌对）
        /// </summary>
        /// <param name="refreshToken">前端传来的刷新令牌</param>
        /// <returns>返回包含新 AccessToken 和新 RefreshToken 的响应对象</returns>
        /// <exception cref="Exception">当刷新令牌无效或已过期时抛出异常</exception>
        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            // 1. 在数据库中查找匹配的 RefreshToken，且该令牌未过期
            var users = await _userRepository.FindAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > DateTime.Now);
            var user = users.FirstOrDefault();

            // 2. 如果找不到匹配的用户或令牌已失效，则拒绝刷新
            if (user == null)
            {
                throw new Exception("刷新令牌无效或已过期");
            }

            // 3. 重新生成一套全新的令牌对
            var tokenResponse = _jwtService.GenerateTokens(user);

            // 4. 更新数据库中的 RefreshToken 记录（实现令牌的轮转更新）
            user.RefreshToken = tokenResponse.RefreshToken;
            user.RefreshTokenExpiryTime = tokenResponse.RefreshTokenExpiresAt;
            await _userRepository.UpdateAsync(user);

            return tokenResponse;
        }

        /// <summary>
        /// 撤销/注销用户的刷新令牌（通常用于用户主动退出登录）
        /// </summary>
        /// <param name="userId">需要注销令牌的用户ID</param>
        /// <returns>操作成功返回 true，用户不存在返回 false</returns>
        public async Task<bool> RevokeRefreshTokenAsync(long userId)
        {
            // 1. 根据 ID 查找用户
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // 2. 清空数据库中的 RefreshToken 及其过期时间，使旧的刷新令牌彻底失效
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userRepository.UpdateAsync(user);

            return true;
        }
    }
}
