using System.ComponentModel.DataAnnotations;

namespace PlanNoteServer.DTOs
{
    /// <summary>
    /// 新增年度目标请求 DTO（UserID 从 JWT 令牌解析，无需前端传入）
    /// </summary>
    public class CreateYearlyGoalRequest
    {
        /// <summary>
        /// 关联标签 ID（可空，为 NULL 表示未打标签）
        /// </summary>
        public long? TagId { get; set; }

        /// <summary>
        /// 年度目标标题（必填，最大 100 字符，如：掌握 Python 编程）
        /// </summary>
        [Required(ErrorMessage = "目标标题不能为空")]
        [MaxLength(100, ErrorMessage = "目标标题不能超过 100 个字符")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 详细描述或关键结果（KR，可空）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 目标年份（必填，如：2024，范围 1970 ~ 2100）
        /// </summary>
        [Required(ErrorMessage = "目标年份不能为空")]
        [Range(1970, 2100, ErrorMessage = "目标年份必须在 1970 ~ 2100 之间")]
        public int TargetYear { get; set; }
    }
}
