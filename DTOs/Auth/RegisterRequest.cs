using System.ComponentModel.DataAnnotations;

namespace PlanNoteServer.DTOs.Auth
{
    /// <summary>
    /// 微信用户注册/信息补充请求
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// 微信唯一标识（openid）
        /// </summary>
        [Required(ErrorMessage = "openid 不能为空")]
        [StringLength(64, ErrorMessage = "openid 长度不能超过64个字符")]
        public string OpenId { get; set; } = string.Empty;

        /// <summary>
        /// 微信开放平台 unionid（可选，多端关联用）
        /// </summary>
        [StringLength(64, ErrorMessage = "unionid 长度不能超过64个字符")]
        public string? UnionId { get; set; }

        /// <summary>
        /// 昵称（可选，未授权时为空）
        /// </summary>
        [StringLength(50, ErrorMessage = "昵称长度不能超过50个字符")]
        public string? NickName { get; set; }

        /// <summary>
        /// 头像地址（可选）
        /// </summary>
        [StringLength(255, ErrorMessage = "头像地址长度不能超过255个字符")]
        public string? AvatarUrl { get; set; }
    }
}
