using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 年度目标表配置（字段类型精度、Check 约束、索引、表名映射）
    /// </summary>
    public class YearlyGoalsConfiguration : IEntityTypeConfiguration<YearlyGoals>
    {
        public void Configure(EntityTypeBuilder<YearlyGoals> builder)
        {
            // 显式映射数据库表名（模型 YearlyGoals → 数据库 yearly_goals）
            // EF Core 9.0+ 要求 Check 约束写在 ToTable(..., t => ...) 的表级 lambda 内，避免过时警告
            builder.ToTable("yearly_goals", t =>
            {
                // ======== Check 约束（数据库层保证数据合法性，防止脏数据写入） ========
                // 进度必须在 0 ~ 100 之间
                t.HasCheckConstraint("CK_yearly_goals_Progress",
                    "Progress >= 0 AND Progress <= 100");

                // 状态枚举值限定（1:进行中, 2:已完成, 3:已放弃）
                t.HasCheckConstraint("CK_yearly_goals_YearlyStatus",
                    "YearlyStatus IN (1, 2, 3)");
            });

            // 主键 ID（BIGINT 自增）
            builder.HasKey(yg => yg.ID);
            builder.Property(yg => yg.ID)
                .UseIdentityColumn();

            // UserID：关联用户，必填（BIGINT，与 Users.Id 一致）
            builder.Property(yg => yg.UserID)
                .IsRequired();

            // Title：年度目标标题，必填，最大长度 100
            builder.Property(yg => yg.Title)
                .IsRequired()
                .HasMaxLength(100);

            // Description：详细描述 / 关键结果，长文本
            // 显式指定数据库列类型 TEXT，EF 默认会映射为 NVARCHAR(MAX)
            builder.Property(yg => yg.Description)
                .IsRequired(false)
                .HasColumnType("TEXT");

            // TargetYear：年份，必填（建议范围 1970-2100，业务层再校验更严格范围）
            builder.Property(yg => yg.TargetYear)
                .IsRequired();

            // Progress：进度百分比 DECIMAL(5,2)，必填，默认 0
            // EF 默认精度是 decimal(18,2)，必须通过 HasPrecision(5,2) 精确对齐字段表要求
            builder.Property(yg => yg.Progress)
                .IsRequired()
                .HasPrecision(5, 2)
                .HasDefaultValue(0m);

            // YearlyStatus：状态 TINYINT，必填，默认 1（进行中）
            builder.Property(yg => yg.YearlyStatus)
                .IsRequired()
                .HasDefaultValue((byte)1);

            // CreatedAt：创建时间，默认当前时间（GETDATE）
            builder.Property(yg => yg.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // ======== 索引 ========
            // 1) 复合索引 (UserID, TargetYear)：最常用查询 —— 「某用户在某一年的所有年度目标」
            //    最左前缀原则下，该索引也能覆盖纯 UserID 单条件查询
            builder.HasIndex(yg => new { yg.UserID, yg.TargetYear })
                .HasDatabaseName("IX_yearly_goals_UserID_TargetYear");

            // 2) TargetYear 单列索引：跨用户统计某年份所有目标（报表场景）
            builder.HasIndex(yg => yg.TargetYear)
                .HasDatabaseName("IX_yearly_goals_TargetYear");

            // 3) (UserID, YearlyStatus) 索引：常见筛选「用户进行中的目标」
            builder.HasIndex(yg => new { yg.UserID, yg.YearlyStatus })
                .HasDatabaseName("IX_yearly_goals_UserID_YearlyStatus");

            // 4) CreatedAt 索引（按时间排序最新目标）
            builder.HasIndex(yg => yg.CreatedAt)
                .HasDatabaseName("IX_yearly_goals_CreatedAt");

            // ======== 外键：UserID → Users.Id（级联删除：删用户时一并清空其所有年度目标） ========
            // 注：因为 Users 使用软删除（IsDeleted），真实 DELETE 很少触发；级联是为了物理删除时不产生孤立记录
            builder.HasOne(yg => yg.User)
                .WithMany()                         // Users 端暂不声明反向导航集合
                .HasForeignKey(yg => yg.UserID)
                .HasConstraintName("FK_yearly_goals_users_UserID")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
