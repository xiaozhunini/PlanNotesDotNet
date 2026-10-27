using System.ComponentModel.DataAnnotations;

namespace PlanNoteServer.DTOs
{
    /// <summary>
    /// 新增目标标签请求 DTO
    /// </summary>
    public class CreateGoalTagRequest
    {
        /// <summary>
        /// 标签名称（必填，最大 50 字符，如：学习、健康）
        /// </summary>
        [Required(ErrorMessage = "标签名称不能为空")]
        [MaxLength(50, ErrorMessage = "标签名称不能超过 50 个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 标签图标 URL（网络链接，最大 255 字符，可空）
        /// </summary>
        [MaxLength(255, ErrorMessage = "图标 URL 不能超过 255 个字符")]
        public string? Icon { get; set; }

        /// <summary>
        /// 标签颜色（十六进制色值，如 #FF6B6B，最大 20 字符，可空）
        /// </summary>
        [MaxLength(20, ErrorMessage = "颜色值不能超过 20 个字符")]
        public string? Color { get; set; }

        /// <summary>
        /// 创建者 ID（0 表示系统通用标签，默认 0）
        /// </summary>
        public long UserID { get; set; } = 0;

        /// <summary>
        /// 排序字段（值越小越靠前，默认 0，必须非负）
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "排序值必须大于等于 0")]
        public int SortOrder { get; set; } = 0;
    }
}
