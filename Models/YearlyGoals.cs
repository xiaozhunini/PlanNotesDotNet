namespace PlanNoteServer.Models
{
    /// <summary>
    /// 年度目标表（每个用户可按年份设定多个年度目标，对应 OKR 中的 Objective + KR）
    /// 对应数据库表：yearly_goals
    /// </summary>
    public class YearlyGoals
    {
        /// <summary>
        /// 主键 ID（BIGINT，自增）
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 关联用户 ID（BIGINT，外键 → Users.Id）
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 年度目标标题（如：掌握 Python 编程）
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 详细描述或关键结果（KR，长文本，对应数据库 TEXT 类型）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 年份（如：2024）
        /// </summary>
        public int TargetYear { get; set; }

        /// <summary>
        /// 当前进度百分比（0-100，可手动填写或由子计划/笔记完成度自动计算）
        /// 数据库类型：DECIMAL(5,2)，保留两位小数，范围 -999.99 ~ 999.99
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

        // 导航属性

        /// <summary>
        /// 导航属性：关联的用户（Include 查询直接拿到用户信息）
        /// </summary>
        public Users? User { get; set; }
    }
}
