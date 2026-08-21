using System.ComponentModel.DataAnnotations;

namespace PlanNoteServer.DTOs.Auth
{
    /// <summary>
    /// 微信登录请求（前端通过 wx.login() 获取 code 后提交）
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// 微信登录凭证（jscode，后端用于调用 jscode2session 换取 openid）
        /// </summary>
        [Required(ErrorMessage = "登录凭证 code 不能为空")]
        [StringLength(128, ErrorMessage = "code 长度不能超过128个字符")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 昵称（可选，微信授权后由前端传回，用于首次登录时补全资料）
        /// </summary>
        [StringLength(50, ErrorMessage = "昵称长度不能超过50个字符")]
        public string? NickName { get; set; }

        /// <summary>
        /// 头像地址（可选，用于首次登录时补全资料）
        /// </summary>
        [StringLength(255, ErrorMessage = "头像地址长度不能超过255个字符")]
        public string? AvatarUrl { get; set; }
    }
}
