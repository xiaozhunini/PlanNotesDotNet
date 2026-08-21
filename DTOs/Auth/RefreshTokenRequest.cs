using System.ComponentModel.DataAnnotations;

namespace PlanNoteServer.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "刷新令牌不能为空")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}