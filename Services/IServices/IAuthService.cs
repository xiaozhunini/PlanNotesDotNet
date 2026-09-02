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
        /// 撤销/注销指定的 RefreshToken（通常用于用户主动退出登录）。
        /// 注意：撤销单条 token 不撤销家族 —— 用户在另一设备仍可使用同家族其他 token。
        /// 如需强制全端下线，应调用 ITokenStore.RevokeFamilyAsync。
        /// </summary>
        /// <param name="refreshToken">要撤销的 RefreshToken 字符串</param>
        /// <returns>令牌存在且已撤销返回 true，令牌不存在返回 false</returns>
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }
}
