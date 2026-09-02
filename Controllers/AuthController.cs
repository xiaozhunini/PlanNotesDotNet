using PlanNoteServer.DTOs;
using PlanNoteServer.DTOs.Auth;
using PlanNoteServer.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PlanNoteServer.Controllers
{
    /// <summary>
    /// 认证控制器（提供微信登录、用户注册、令牌刷新、登出及获取当前用户信息等接口）
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// 构造函数，通过依赖注入获取认证服务和日志记录器
        /// </summary>
        /// <param name="authService">认证业务服务</param>
        /// <param name="logger">日志记录器</param>
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 用户注册接口（基于 openid 直接创建用户记录）
        /// </summary>
        /// <param name="request">包含 openid 及可选资料的注册请求</param>
        /// <returns>注册成功后返回创建的用户信息</returns>
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // 校验模型绑定状态（如必填项、格式校验等）
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _authService.RegisterAsync(request);
                // 返回 201 Created 状态码，并附带用户信息
                return CreatedAtAction(nameof(GetCurrentUser), user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册失败");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 微信登录接口（通过 wx.login 的 code 登录，用户不存在则自动注册）
        /// </summary>
        /// <param name="request">包含 code 的登录请求</param>
        /// <returns>登录成功后返回包含 AccessToken 和 RefreshToken 的响应对象</returns>
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tokenResponse = await _authService.LoginAsync(request);
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录失败");
                // 登录失败返回 401 Unauthorized 状态码
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 刷新令牌接口（使用长效的 RefreshToken 换取新的令牌对）
        /// </summary>
        /// <param name="request">包含 RefreshToken 的请求对象</param>
        /// <returns>返回包含新 AccessToken 和新 RefreshToken 的响应对象</returns>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tokenResponse = await _authService.RefreshTokenAsync(request.RefreshToken);
                return Ok(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新令牌失败");
                // 刷新令牌失败（如令牌无效或过期）返回 401 状态码
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 用户登出接口（需要携带有效的 AccessToken）。
        /// 前端需在 Body 中传 RefreshToken，登出会让该 RefreshToken 立即失效。
        /// 注意：AccessToken 本身是无状态的，登出后到过期前仍可用，如需立即失效需要 AccessToken 黑名单（后续阶段实现）。
        /// </summary>
        [HttpPost("logout")]
        [Authorize] // 必须通过 JWT 认证才能访问
        public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 撤销 RefreshToken（写入 Redis 标记为 revoked）
                var ok = await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
                if (!ok)
                {
                    // 即使 RefreshToken 不存在（可能已过期或已撤销），登出仍视为成功
                    // 不应向用户泄露令牌状态信息
                }
                return Ok(new { message = "登出成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登出失败");
                return StatusCode(500, new { message = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 获取当前登录用户信息接口（需要携带有效的 AccessToken）
        /// </summary>
        /// <returns>返回当前用户的详细信息 DTO</returns>
        [HttpGet("me")]
        [Authorize] // 必须通过 JWT 认证才能访问
        public ActionResult<UserDto> GetCurrentUser()
        {
            try
            {
                // 从 Token 的 Claims 中解析出当前用户的各项信息
                var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var openId = User.FindFirst("openid")?.Value ?? string.Empty;
                var nickName = User.FindFirst("nickname")?.Value ?? string.Empty;
                var roleTypeStr = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var statusStr = User.FindFirst("status")?.Value;

                // 角色类型与状态为数值字符串，需转换为 byte
                byte roleType = byte.TryParse(roleTypeStr, out var rt) ? rt : (byte)1;
                byte status = byte.TryParse(statusStr, out var st) ? st : (byte)0;

                return Ok(new UserDto
                {
                    Id = userId,
                    OpenId = openId,
                    NickName = nickName,
                    RoleType = roleType,
                    UserStatus = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取当前用户信息失败");
                return StatusCode(500, new { message = "服务器内部错误" });
            }
        }
    }
}
