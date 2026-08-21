using PlanNoteServer.DTOs;
using PlanNoteServer.DTOs.Auth;

namespace PlanNoteServer.Services.IServices
{
    /// <summary>
    /// 认证服务接口（微信登录、用户注册、令牌刷新、登出等认证业务）
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 微信用户注册/创建（基于 openid 直接创建用户记录）
        /// </summary>
        /// <param name="request">包含 openid 及可选资料的注册请求</param>
        /// <returns>注册成功后返回的用户信息 DTO</returns>
        Task<UserDto> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// 微信登录（通过 wx.login 的 code 登录，用户不存在则自动注册）
        /// </summary>
        /// <param name="request">包含 code 的登录请求</param>
        /// <returns>登录成功后返回包含 AccessToken 和 RefreshToken 的响应</returns>
        Task<TokenResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// 刷新令牌（用 RefreshToken 换取新令牌对）
        /// </summary>
        /// <param name="refreshToken">前端传来的刷新令牌</param>
        /// <returns>返回包含新 AccessToken 和新 RefreshToken 的响应</returns>
        Task<TokenResponse> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// 撤销/注销用户的刷新令牌（通常用于主动退出登录）
        /// </summary>
        /// <param name="userId">需要注销的用户ID</param>
        /// <returns>操作成功返回 true，用户不存在返回 false</returns>
        Task<bool> RevokeRefreshTokenAsync(long userId);
    }
}
