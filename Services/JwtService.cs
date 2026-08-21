using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using PlanNoteServer.Configuration;
using PlanNoteServer.DTOs.Auth;
using PlanNoteServer.Models;
using PlanNoteServer.Services.IServices;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PlanNoteServer.Services
{
    /// <summary>
    /// JWT 服务实现类（生成 Token、验证 Token、提取 Token 携带信息）
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly TokenValidationParameters _tokenValidationParameters;

        /// <summary>
        /// 构造函数，注入配置并初始化 Token 验证参数
        /// </summary>
        /// <param name="jwtSettings">从 appsettings.json 映射的 JWT 配置选项</param>
        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;

            // 初始化 Token 验证参数，规定在校验 Token 时需要检查的项目
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,               // 校验签发者
                ValidateAudience = true,             // 校验受众
                ValidateLifetime = true,             // 校验有效期
                ValidateIssuerSigningKey = true,     // 校验签名密钥
                ValidIssuer = _jwtSettings.Issuer,   // 合法的签发者
                ValidAudience = _jwtSettings.Audience, // 合法的受众
                // 将配置中的 SecretKey 转换为对称安全密钥，用于签名校验
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero            // 取消默认的5分钟时钟偏差，要求严格的时效校验
            };
        }

        /// <summary>
        /// 生成令牌对（短效 AccessToken + 长效 RefreshToken）
        /// </summary>
        /// <param name="user">当前登录的用户实体</param>
        /// <returns>包含两种令牌字符串及其过期时间的响应对象</returns>
        public TokenResponse GenerateTokens(Users user)
        {
            // 生成短效的访问令牌和长效的刷新令牌
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // 根据配置计算两种令牌的过期时间
            var accessTokenExpiresAt = DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
            var refreshTokenExpiresAt = DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
                TokenType = "Bearer" // 指定令牌类型为 Bearer
            };
        }

        /// <summary>
        /// 生成短效的 AccessToken（访问令牌）
        /// </summary>
        /// <param name="user">当前登录的用户实体</param>
        /// <returns>签名后的 JWT 字符串</returns>
        public string GenerateAccessToken(Users user)
        {
            // 构建 Claims 声明集合，携带用户的核心身份信息放入 Token 中
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.OpenId),
                new Claim("openid", user.OpenId),
                new Claim("nickname", user.NickName ?? string.Empty),
                new Claim(ClaimTypes.Role, user.RoleType.ToString()),
                new Claim("status", user.UserStatus.ToString())
            };

            // 使用配置的 SecretKey 创建签名凭证
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 封装 JWT 安全令牌对象
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: creds
            );

            // 将令牌对象序列化为紧凑的 JWT 字符串
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 生成安全的 RefreshToken（刷新令牌）
        /// </summary>
        /// <returns>Base64 编码的随机字符串</returns>
        public string GenerateRefreshToken()
        {
            // 使用加密随机数生成器生成 64 字节的强随机数，确保 RefreshToken 无法被预测
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// 验证 Token 是否合法（含签名、有效期等校验）
        /// </summary>
        /// <param name="token">待验证的 JWT 字符串</param>
        /// <returns>验证通过返回 true，失败返回 false</returns>
        public bool ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                // 使用构造函数中配置好的验证参数进行严格校验
                tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
                return true;
            }
            catch
            {
                // 任何校验失败（如过期、签名错误）都会抛出异常，这里捕获并返回 false
                return false;
            }
        }

        /// <summary>
        /// 验证 Token 并提取其中的核心信息（用户ID、openid）
        /// </summary>
        /// <param name="token">待验证的 JWT 字符串</param>
        /// <returns>元组：(是否有效, 用户ID, openid)</returns>
        public (bool IsValid, long? UserId, string? OpenId) ValidateAndExtractClaims(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                // 验证 Token 并提取 ClaimsPrincipal，其中包含声明的所有信息
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);

                // 从声明集合中提取指定的用户信息
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                var openIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "openid");

                return (
                    IsValid: true,
                    UserId: userIdClaim != null ? long.Parse(userIdClaim.Value) : null,
                    OpenId: openIdClaim?.Value
                );
            }
            catch
            {
                // 验证失败时返回无效的默认值
                return (IsValid: false, UserId: null, OpenId: null);
            }
        }

        /// <summary>
        /// 获取 Token 的过期时间
        /// </summary>
        /// <param name="token">JWT 字符串</param>
        /// <returns>Token 的过期时间（ValidTo），解析失败返回 DateTime.MinValue</returns>
        public DateTime GetTokenExpiration(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                // 读取 JWT 令牌（此方法不验证签名，仅解析结构）
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}
