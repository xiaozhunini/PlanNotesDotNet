using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 周计划表配置（DATE 类型精确映射、可选外键、组合唯一约束、索引、表名映射）
    /// </summary>
    public class WeeklyPlansConfiguration : IEntityTypeConfiguration<WeeklyPlans>
    {
        public void Configure(EntityTypeBuilder<WeeklyPlans> builder)
        {
            // 显式映射数据库表名（模型 WeeklyPlans → 数据库下划线 weekly_plans）
            // Check 约束（日期合法性）放在表级 lambda 内（EF Core 9.0+ 新写法避免过时警告）
            builder.ToTable("weekly_plans", t =>
            {
                // 日期合法性 Check：开始日期必须早于结束日期
                t.HasCheckConstraint("CK_weekly_plans_StartBeforeEnd",
                    "WeekStartDate < WeekEndDate");
            });

            // 主键 ID（BIGINT 自增）
            builder.HasKey(wp => wp.ID);
            builder.Property(wp => wp.ID)
                .UseIdentityColumn();

            // UserID：关联用户 ID，必填，BIGINT（与 Users.Id 类型一致）
            builder.Property(wp => wp.UserID)
                .IsRequired();

            // YearlyGoalID：关联年计划 ID，**可空** BIGINT（允许周计划不绑定到任何年计划）
            builder.Property(wp => wp.YearlyGoalID)
                .IsRequired(false);

            // ======== DATE 类型字段（关键：必须显式 .HasColumnType("DATE") 对齐你给的 DATE 类型） ========
            // 经验 683034：EF Core 默认把 DateTime 映射成 DATETIME2 / DATETIME，
            // 若字段表要求纯 DATE，不显式指定会导致列类型错误、读取时带时分秒 00:00:00 没问题但存储结构不匹配。

            // WeekStartDate：本周开始日期（纯 DATE，必填）
            builder.Property(wp => wp.WeekStartDate)
                .IsRequired()
                .HasColumnType("DATE");

            // WeekEndDate：本周结束日期（纯 DATE，必填）
            builder.Property(wp => wp.WeekEndDate)
                .IsRequired()
                .HasColumnType("DATE");

            // Content：周计划内容（支持富文本 / JSON 数组），TEXT 类型长文本，可空
            builder.Property(wp => wp.Content)
                .IsRequired(false)
                .HasColumnType("TEXT");

            // Review：周复盘总结，TEXT 类型长文本，可空
            builder.Property(wp => wp.Review)
                .IsRequired(false)
                .HasColumnType("TEXT");

            // CreatedAt：创建时间，默认 GETDATE()
            builder.Property(wp => wp.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // ======== 索引 ========
            // 1) (UserID, WeekStartDate) 组合 **唯一索引**：同一用户同一周不能有两份周计划
            builder.HasIndex(wp => new { wp.UserID, wp.WeekStartDate })
                .IsUnique()
                .HasDatabaseName("UQ_weekly_plans_UserID_WeekStartDate");

            // 2) (UserID, WeekEndDate) 普通索引：按结束日期范围筛选（例如「本月已结束的周计划」）
            builder.HasIndex(wp => new { wp.UserID, wp.WeekEndDate })
                .HasDatabaseName("IX_weekly_plans_UserID_WeekEndDate");

            // 3) YearlyGoalID 索引：查询「某年度目标相关的所有周计划」（常用于年目标进度汇总）
            builder.HasIndex(wp => wp.YearlyGoalID)
                .HasDatabaseName("IX_weekly_plans_YearlyGoalID");

            // 4) CreatedAt 索引：按创建时间排序
            builder.HasIndex(wp => wp.CreatedAt)
                .HasDatabaseName("IX_weekly_plans_CreatedAt");

            // ======== 外键 ========
            // FK1：UserID → Users.Id（级联删除：删除用户时一并删除其所有周计划）
            builder.HasOne(wp => wp.User)
                .WithMany()
                .HasForeignKey(wp => wp.UserID)
                .HasConstraintName("FK_weekly_plans_users_UserID")
                .OnDelete(DeleteBehavior.Cascade);

            // FK2：YearlyGoalID → yearly_goals.ID（可空外键）
            // 删除策略：ClientSetNull —— 删除年度目标时，不删对应的周计划，仅把周计划的 YearlyGoalID 置为 NULL，保留历史周计划不丢
            builder.HasOne(wp => wp.YearlyGoal)
                .WithMany()
                .HasForeignKey(wp => wp.YearlyGoalID)
                .HasConstraintName("FK_weekly_plans_yearly_goals_YearlyGoalID")
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
