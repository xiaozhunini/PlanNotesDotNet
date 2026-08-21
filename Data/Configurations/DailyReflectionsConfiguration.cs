using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 每日复盘表配置（DATE 精确映射、UserID+PlanDate 组合唯一索引、DECIMAL 精度、表名映射）
    /// </summary>
    public class DailyReflectionsConfiguration : IEntityTypeConfiguration<DailyReflections>
    {
        public void Configure(EntityTypeBuilder<DailyReflections> builder)
        {
            // 表名映射（模型 DailyReflections → 数据库下划线 daily_reflections）
            // CHECK 约束放在表级 lambda 内（EF Core 9.0+ 写法）
            builder.ToTable("daily_reflections", t =>
            {
                // 当日平均分若填写则必须在 0~10 之间（可空约束已通过字段 IsRequired(false) 控制）
                t.HasCheckConstraint("CK_daily_reflections_TotalScoreAvg",
                    "TotalScoreAvg IS NULL OR TotalScoreAvg BETWEEN 0 AND 10");
            });

            // 主键 ID（BIGINT 自增）
            builder.HasKey(dr => dr.ID);
            builder.Property(dr => dr.ID)
                .UseIdentityColumn();

            // UserID：关联用户，必填，BIGINT（与 Users.Id 类型一致）
            builder.Property(dr => dr.UserID)
                .IsRequired();

            // PlanDate：复盘日期，纯 DATE（显式指定避免 EF 默认 DATETIME2，经验 683034）
            builder.Property(dr => dr.PlanDate)
                .IsRequired()
                .HasColumnType("DATE");

            // MorningReview：上午反思，TEXT 长文本，可空
            builder.Property(dr => dr.MorningReview)
                .IsRequired(false)
                .HasColumnType("TEXT");

            // AfternoonReview：下午反思，TEXT 长文本，可空
            builder.Property(dr => dr.AfternoonReview)
                .IsRequired(false)
                .HasColumnType("TEXT");

            // EveningSummary：晚上总结，TEXT 长文本，可空
            builder.Property(dr => dr.EveningSummary)
                .IsRequired(false)
                .HasColumnType("TEXT");

            // TotalScoreAvg：当日平均得分（可选 DECIMAL(4,2)）
            // 精度说明：评分单项 1~10，平均分可能刚好 10.00，原 DECIMAL(3,2) 整数部分只有 1 位（最大 9.99）无法存 10.00
            // 因此采用 DECIMAL(4,2)：总长 4 位、小数 2 位（-99.99 ~ 99.99），CHECK 约束再限制为 0~10，既满足 10.00 又不会溢出
            builder.Property(dr => dr.TotalScoreAvg)
                .IsRequired(false)
                .HasPrecision(4, 2);

            // ======== 索引 ========
            // 1) (UserID, PlanDate) 组合 **唯一索引**：保证"1 个用户 1 天只能 1 条复盘"
            //    经验 1393833：按 user_id + date 组合唯一防重复写入（若只做 PlanDate 单列唯一会导致全用户抢同一天，明显错误）
            //    最左前缀原则下该索引还能覆盖纯 UserID 单条件查询（"我所有历史复盘"）
            builder.HasIndex(dr => new { dr.UserID, dr.PlanDate })
                .IsUnique()
                .HasDatabaseName("UQ_daily_reflections_UserID_PlanDate");

            // 2) PlanDate 单列索引：按日期跨用户统计（报表场景）
            builder.HasIndex(dr => dr.PlanDate)
                .HasDatabaseName("IX_daily_reflections_PlanDate");

            // 3) TotalScoreAvg 单列索引（筛选平均分较高/较低的日子，可选但常用，低基数不影响性能）
            builder.HasIndex(dr => dr.TotalScoreAvg)
                .HasDatabaseName("IX_daily_reflections_TotalScoreAvg");

            // ======== 外键 ========
            // UserID → Users.Id（级联删除：删除用户时一并删除其所有复盘）
            builder.HasOne(dr => dr.User)
                .WithMany()
                .HasForeignKey(dr => dr.UserID)
                .HasConstraintName("FK_daily_reflections_users_UserID")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
