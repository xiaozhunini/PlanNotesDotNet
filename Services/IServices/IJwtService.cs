using PlanNoteServer.DTOs.Auth;
using PlanNoteServer.Models;

namespace PlanNoteServer.Services.IServices
{
    /// <summary>
    /// JWT 服务接口（Token 生成、验证、信息提取等）
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// 生成令牌对（短效 AccessToken + 长效 RefreshToken）
        /// </summary>
        /// <param name="user">当前登录的用户实体</param>
        /// <returns>包含两种令牌字符串及其过期时间的响应对象</returns>
        TokenResponse GenerateTokens(Users user);

        /// <summary>
        /// 生成短效 AccessToken（访问令牌）
        /// </summary>
        /// <param name="user">当前登录的用户实体</param>
        /// <returns>签名后的 JWT 字符串</returns>
        string GenerateAccessToken(Users user);

        /// <summary>
        /// 生成安全的 RefreshToken（刷新令牌）
        /// </summary>
        /// <returns>Base64 编码的随机字符串</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// 验证 Token 是否合法（含签名、有效期等校验）
        /// </summary>
        /// <param name="token">待验证的 JWT 字符串</param>
        /// <returns>验证通过返回 true，失败返回 false</returns>
        bool ValidateToken(string token);

        /// <summary>
        /// 验证 Token 并提取其中的核心信息（用户ID、openid）
        /// </summary>
        /// <param name="token">待验证的 JWT 字符串</param>
        /// <returns>元组：(是否有效, 用户ID, openid)</returns>
        (bool IsValid, long? UserId, string? OpenId) ValidateAndExtractClaims(string token);

        /// <summary>
        /// 获取 Token 的过期时间
        /// </summary>
        /// <param name="token">JWT 字符串</param>
        /// <returns>Token 的过期时间，解析失败返回 DateTime.MinValue</returns>
        DateTime GetTokenExpiration(string token);
    }
}
