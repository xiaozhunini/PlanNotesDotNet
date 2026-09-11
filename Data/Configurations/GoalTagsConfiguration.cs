using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 目标标签表配置（系统通用标签 + 用户自建标签、字段长度约束、排序 Check、表名映射）
    /// </summary>
    public class GoalTagsConfiguration : IEntityTypeConfiguration<GoalTags>
    {
        public void Configure(EntityTypeBuilder<GoalTags> builder)
        {
            // 显式映射数据库表名（模型 GoalTags → 数据库 goal_tag）
            builder.ToTable("goal_tag", t =>
            {
                // SortOrder 必须非负（排序权重不允许负数）
                t.HasCheckConstraint("CK_goal_tag_SortOrder",
                    "SortOrder >= 0");
            });

            // 主键 ID（BIGINT 自增）
            builder.HasKey(gt => gt.ID);
            builder.Property(gt => gt.ID)
                .UseIdentityColumn();

            // Name：标签名称，必填，最大长度 50，唯一索引（防止重复标签）
            builder.Property(gt => gt.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);  // VARCHAR 存储，标签名一般为英文/中文短词

            // Icon：标签图标 URL 或图标名，可空，最大长度 255
            builder.Property(gt => gt.Icon)
                .IsRequired(false)
                .HasMaxLength(255);

            // Color：标签颜色（如 #FF6B6B），可空，最大长度 20
            builder.Property(gt => gt.Color)
                .IsRequired(false)
                .HasMaxLength(20)
                .IsUnicode(false);  // 十六进制色值，纯 ASCII

            // UserID：创建者 ID，必填，默认 0（0 = 系统通用标签；非 0 = 用户自建，软关联 Users.Id）
            builder.Property(gt => gt.UserID)
                .IsRequired()
                .HasDefaultValue(0L);

            // SortOrder：排序权重，必填，默认 0（值越小越靠前，Check 约束保证非负）
            builder.Property(gt => gt.SortOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // CreatedAt：创建时间，默认 GETDATE()
            builder.Property(gt => gt.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // ======== 索引 ========
            // 1) (UserID, Name) 组合唯一索引：同一创建者下标签名不能重复
            //    系统标签（UserID=0）之间不重名；不同用户允许各自建同名标签（如各自都有"学习"）
            builder.HasIndex(gt => new { gt.UserID, gt.Name })
                .IsUnique()
                .HasDatabaseName("UQ_goal_tag_UserID_Name");

            // 2) (UserID, SortOrder) 普通索引：高频查询「某用户可见的标签（系统通用 + 自建）并按排序展示」
            builder.HasIndex(gt => new { gt.UserID, gt.SortOrder })
                .HasDatabaseName("IX_goal_tag_UserID_SortOrder");

            // 3) CreatedAt 索引：按创建时间排序
            builder.HasIndex(gt => gt.CreatedAt)
                .HasDatabaseName("IX_goal_tag_CreatedAt");
        }
    }
}
