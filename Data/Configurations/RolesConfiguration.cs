using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 角色实体配置（字段长度、索引、默认值等约束）
    /// </summary>
    public class RolesConfiguration : IEntityTypeConfiguration<Roles>
    {
        public void Configure(EntityTypeBuilder<Roles> builder)
        {
            // 表名
            builder.ToTable("Roles");

            // 主键
            builder.HasKey(r => r.Id);

            // Id 自增（INT 类型）
            builder.Property(r => r.Id)
                .UseIdentityColumn();

            // RolesName：角色标识，必填，最大长度 20，唯一索引（避免重复角色名）
            builder.Property(r => r.RolesName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            builder.HasIndex(r => r.RolesName).IsUnique();

            // DisplayName：角色显示名称，必填，最大长度 50
            builder.Property(r => r.DisplayName)
                .IsRequired()
                .HasMaxLength(50);

            // Description：角色描述，可选，最大长度 200
            builder.Property(r => r.Description)
                .IsRequired(false)
                .HasMaxLength(200);

            // CreatedAt：创建时间，必填，默认当前时间
            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // 创建时间索引
            builder.HasIndex(r => r.CreatedAt);
        }
    }
}
