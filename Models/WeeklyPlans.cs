namespace PlanNoteServer.Models
{
    /// <summary>
    /// 周计划表（每个用户每周一份计划，可关联到某个年度目标）
    /// 对应数据库表：weekly_plans
    /// </summary>
    public class WeeklyPlans
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
        /// 关联年计划 ID（BIGINT，可空，外键 → yearly_goals.ID）
        /// 可选，表明本周重点是为了推进哪个年度目标
        /// </summary>
        public long? YearlyGoalID { get; set; }

        /// <summary>
        /// 本周开始日期（通常为周一，数据库仅存 DATE，不存时分秒）
        /// </summary>
        public DateTime WeekStartDate { get; set; }

        /// <summary>
        /// 本周结束日期（通常为周日，数据库仅存 DATE，不存时分秒）
        /// </summary>
        public DateTime WeekEndDate { get; set; }

        /// <summary>
        /// 周计划内容列表（支持富文本或 JSON 数组，具体格式由业务层定义）
        /// 对应数据库 TEXT 类型，长文本
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// 周复盘 / 总结（周末填写，对应数据库 TEXT 类型长文本）
        /// </summary>
        public string? Review { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // ===== 导航属性 =====

        /// <summary>
        /// 导航属性：关联的用户
        /// </summary>
        public Users? User { get; set; }

        /// <summary>
        /// 导航属性：关联的年度目标（可空，未关联时为 null）
        /// </summary>
        public YearlyGoals? YearlyGoal { get; set; }
    }
}
