namespace PlanNoteServer.Configuration
{
    /// <summary>
    /// jwt配置类，program里面封装了，直接注入就能使用类了
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;
        /// <summary>
        /// 签发这
        /// </summary>
        public string Issuer { get; set; } = string.Empty;
        /// <summary>
        /// 受众人
        /// </summary>
        public string Audience { get; set; } = string.Empty;
        /// <summary>
        /// 令牌过期时间
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 30;
        /// <summary>
        /// 令牌刷新天数
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}