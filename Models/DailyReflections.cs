namespace PlanNoteServer.Models
{
    /// <summary>
    /// 每日复盘表（用户一日三阶段反思 + 当日平均得分，每个用户每天 1 条记录）
    /// 对应数据库表：daily_reflections
    /// </summary>
    public class DailyReflections
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
        /// 复盘日期（纯 DATE，无时分秒；与 UserID 组合唯一，保证每个用户每天只存 1 条）
        /// </summary>
        public DateTime PlanDate { get; set; }

        /// <summary>
        /// 上午反思（TEXT 长文本）
        /// </summary>
        public string? MorningReview { get; set; }

        /// <summary>
        /// 下午反思（TEXT 长文本）
        /// </summary>
        public string? AfternoonReview { get; set; }

        /// <summary>
        /// 晚上反思 / 一日总结收获（TEXT 长文本）
        /// </summary>
        public string? EveningSummary { get; set; }

        /// <summary>
        /// 当日所有事项评分的平均分（可选，NULL 表示当日事项没打过分）
        /// 数据类型：DECIMAL(4,2)（注：原 DECIMAL(3,2) 总长度 3 位整数只能存 1 位，最大值 9.99，无法存 10.00 分，因此扩为 4,2 仍保留小数点后 2 位）
        /// </summary>
        public decimal? TotalScoreAvg { get; set; }

        // ===== 导航属性 =====

        /// <summary>
        /// 导航属性：关联的用户
        /// </summary>
        public Users? User { get; set; }
    }
}
