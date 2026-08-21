namespace PlanNoteServer.Models
{
    /// <summary>
    /// 微信用户实体（小程序用户信息）
    /// </summary>
    public class Users : BaseEntity
    {
        /// <summary>
        /// 微信唯一标识（openid）
        /// </summary>
        public string OpenId { get; set; } = string.Empty;

        /// <summary>
        /// 微信开放平台 unionid（如有多端需求）
        /// </summary>
        public string? UnionId { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = string.Empty;

        /// <summary>
        /// 头像地址
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// 手机号（加密存储，数据库存放密文）
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 角色类型（1普通用户/2编辑/3管理员）
        /// </summary>
        public byte RoleType { get; set; } = 1;

        /// <summary>
        /// 账号状态（0正常/1禁用）
        /// </summary>
        public byte UserStatus { get; set; } = 0;

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 当前刷新令牌（用于令牌轮转，登出时清空）
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// 刷新令牌过期时间
        /// </summary>
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
