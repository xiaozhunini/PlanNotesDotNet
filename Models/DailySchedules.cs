namespace PlanNoteServer.Models
{
    /// <summary>
    /// 每日日程表（用户按天细分的时间表项，用于每日打卡/复盘场景）
    /// 对应数据库表：daily_schedules
    /// </summary>
    public class DailySchedules
    {
        /// <summary>
        /// 主键 ID（BIGINT，自增）
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 关联用户 ID（BIGINT，外键 → Users.Id，必填）
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 日程日期（如 2023-10-27，纯 DATE，无时分秒）
        /// </summary>
        public DateTime PlanDate { get; set; }

        /// <summary>
        /// 开始时间（如 07:20:00，对应数据库 TIME 类型，C# 映射为 TimeSpan）
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// 结束时间（如 08:20:00，对应数据库 TIME 类型）
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// 事项内容（如“起床”、“背单词”，最大长度 255）
        /// </summary>
        public string TaskContent { get; set; } = string.Empty;

        /// <summary>
        /// 是否已完成（True=已完成 / False=未完成）
        /// 新建事项默认为未完成
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// 自我评分（1 ~ 10 分，可空 —— 未完成或未打分时为 NULL）
        /// </summary>
        public int? SelfScore { get; set; }

        /// <summary>
        /// 排序权重（用于同一天内多个日程的显示顺序，数值越小越靠前，默认 0）
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // ===== 导航属性 =====

        /// <summary>
        /// 导航属性：关联的用户
        /// </summary>
        public Users? User { get; set; }
    }
}
