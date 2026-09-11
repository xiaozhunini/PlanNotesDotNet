namespace PlanNoteServer.Models
{
    /// <summary>
    /// 目标标签实体（字典表：给年度目标打标签，如"学习"、"健康"、"生活"）
    /// 对应数据库表：goal_tag
    /// </summary>
    public class GoalTags
    {
        /// <summary>
        /// 主键 ID（BIGINT，自增）
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 标签名称（如：学习、健康）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 标签图标 URL 或图标名（前端图标库的图标名，如 "icon-study"）
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 标签颜色（前端十六进制色值，如 "#FF6B6B"）
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// 创建者 ID（0 表示系统通用标签；非 0 为用户自建标签）
        /// 软关联 Users.Id，不建外键约束（0 是合法值但在 users 表中不存在）
        /// </summary>
        public long UserID { get; set; } = 0;

        /// <summary>
        /// 排序字段（值越小越靠前，默认 0）
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
