namespace PlanNoteServer.DTOs
{
    /// <summary>
    /// 微信用户信息 DTO
    /// </summary>
    public class UserDto : IBaseDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 微信唯一标识（openid）
        /// </summary>
        public string OpenId { get; set; } = string.Empty;

        /// <summary>
        /// 微信开放平台 unionid
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
        /// 手机号（脱敏后返回，如 138****8888）
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 角色类型（1普通用户/2编辑/3管理员）
        /// </summary>
        public byte RoleType { get; set; }

        /// <summary>
        /// 账号状态（0正常/1禁用）
        /// </summary>
        public byte UserStatus { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
