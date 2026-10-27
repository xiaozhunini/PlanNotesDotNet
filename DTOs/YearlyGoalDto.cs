namespace PlanNoteServer.DTOs
{
    /// <summary>
    /// 年度目标标签 DTO（与年计划一并返回，展示标签名/图标/颜色）
    /// </summary>
    public class GoalTagDto
    {
        /// <summary>
        /// 标签 ID
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 标签名称（如：学习、健康）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 标签图标 URL 或图标名
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 标签颜色（十六进制色值，如 #FF6B6B）
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// 创建者 ID（0 = 系统通用标签）
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 排序字段（值越小越靠前）
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// 年度目标 DTO（含关联的标签信息）
    /// </summary>
    public class YearlyGoalDto
    {
        /// <summary>
        /// 主键 ID
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 关联用户 ID
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 关联标签 ID（为 NULL 表示未打标签）
        /// </summary>
        public long? TagId { get; set; }

        /// <summary>
        /// 年度目标标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 详细描述或关键结果（KR）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 年份（如：2024）
        /// </summary>
        public int TargetYear { get; set; }

        /// <summary>
        /// 当前进度百分比（0-100）
        /// </summary>
        public decimal Progress { get; set; }

        /// <summary>
        /// 状态（1：进行中 / 2：已完成 / 3：已放弃）
        /// </summary>
        public byte YearlyStatus { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 关联的标签信息（Include 加载，未打标签时为 null）
        /// </summary>
        public GoalTagDto? Tag { get; set; }
    }
}
