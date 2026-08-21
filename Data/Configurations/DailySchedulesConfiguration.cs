using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 每日日程表配置（DATE + TIME 精确类型映射、Check 约束、索引、表名映射）
    /// </summary>
    public class DailySchedulesConfiguration : IEntityTypeConfiguration<DailySchedules>
    {
        public void Configure(EntityTypeBuilder<DailySchedules> builder)
        {
            // 表名映射（模型 DailySchedules → 数据库下划线 daily_schedules）
            // 所有 Check 约束放在表级 lambda 内，避免 EF Core 9.0+ 过时警告
            builder.ToTable("daily_schedules", t =>
            {
                // Check 1：开始时间必须早于结束时间（同一天内，TIME 比较无歧义）
                t.HasCheckConstraint("CK_daily_schedules_StartBeforeEnd",
                    "StartTime < EndTime");

                // Check 2：自我评分若填写必须在 1~10 之间；未填（NULL）不触发约束
                t.HasCheckConstraint("CK_daily_schedules_SelfScore",
                    "SelfScore IS NULL OR SelfScore BETWEEN 1 AND 10");

                // Check 3：排序权重不能为负数
                t.HasCheckConstraint("CK_daily_schedules_SortOrder",
                    "SortOrder >= 0");
            });

            // 主键 ID（BIGINT 自增）
            builder.HasKey(ds => ds.ID);
            builder.Property(ds => ds.ID)
                .UseIdentityColumn();

            // UserID：关联用户，必填，BIGINT（与 Users.Id 一致）
            builder.Property(ds => ds.UserID)
                .IsRequired();

            // PlanDate：日程日期，纯 DATE（经验 683034 要求显式指定，EF 默认会映射成 DATETIME2）
            builder.Property(ds => ds.PlanDate)
                .IsRequired()
                .HasColumnType("DATE");

            // StartTime：开始时间，TIME 类型
            // C# 的 TimeSpan 直接映射 SQL Server 的 TIME(7)（7 位小数秒，精度最高 100 纳秒，远高于日常 HH:mm:ss 所需）
            builder.Property(ds => ds.StartTime)
                .IsRequired()
                .HasColumnType("TIME");

            // EndTime：结束时间，TIME 类型
            builder.Property(ds => ds.EndTime)
                .IsRequired()
                .HasColumnType("TIME");

            // TaskContent：事项内容，必填，最大长度 255（非 Unicode → VARCHAR(255)）
            builder.Property(ds => ds.TaskContent)
                .IsRequired()
                .HasMaxLength(255);

            // IsCompleted：是否完成，SQL Server 通过 BIT 存储（BOOLEAN 在 SQL Server 是 BIT 的别名），默认 FALSE（未完成）
            builder.Property(ds => ds.IsCompleted)
                .IsRequired()
                .HasDefaultValue(false);

            // SelfScore：自我评分，可空 INT，1~10（CHECK 约束在表级已定义）
            builder.Property(ds => ds.SelfScore)
                .IsRequired(false);

            // SortOrder：排序权重，默认 0（按插入顺序、未调整顺序时统一默认）
            builder.Property(ds => ds.SortOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // CreatedAt：创建时间，默认 GETDATE()
            builder.Property(ds => ds.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // ======== 索引 ========
            // 1) 组合索引 (UserID, PlanDate, SortOrder)：核心查询 —— 某用户某一天所有日程按 SortOrder 排序
            //    最左前缀能覆盖纯 UserID / (UserID, PlanDate) 查询
            builder.HasIndex(ds => new { ds.UserID, ds.PlanDate, ds.SortOrder })
                .HasDatabaseName("IX_daily_schedules_UserID_PlanDate_SortOrder");

            // 2) (UserID, PlanDate, IsCompleted)：常见筛选 —— 用户某天未完成事项 / 已完成事项
            builder.HasIndex(ds => new { ds.UserID, ds.PlanDate, ds.IsCompleted })
                .HasDatabaseName("IX_daily_schedules_UserID_PlanDate_IsCompleted");

            // 3) CreatedAt：按创建时间排序
            builder.HasIndex(ds => ds.CreatedAt)
                .HasDatabaseName("IX_daily_schedules_CreatedAt");

            // ======== 外键 ========
            // UserID → Users.Id（级联删除：删除用户时一并删除其所有日程）
            builder.HasOne(ds => ds.User)
                .WithMany()
                .HasForeignKey(ds => ds.UserID)
                .HasConstraintName("FK_daily_schedules_users_UserID")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
