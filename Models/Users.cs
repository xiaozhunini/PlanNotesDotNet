namespace PlanNoteServer.Models
{
    /// <summary>
    /// 微信用户实体（小程序用户信息）
    /// </summary>
    public class Users
    {
        // ===== 公共字段（原 BaseEntity 内联到此处）=====

        /// <summary>
        /// 主键 ID（BIGINT，自增）
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 创建时间（注册时间）
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 是否删除（软删除标记，默认 false）
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        // ===== 业务字段 =====

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
        /// 账号状态（0正常/1禁用）
        /// </summary>
        public byte UserStatus { get; set; } = 0;

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        // 角色已迁移到 user_roles 关联表（多对多），Users 表不再存 RoleType。
        // RefreshToken 已迁移到 Redis（RedisTokenStore），Users 表不再存令牌相关字段。
    }
}
