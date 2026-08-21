using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 权限实体配置（字段长度、唯一索引等约束）
    /// </summary>
    public class PermissionsConfiguration : IEntityTypeConfiguration<Permissions>
    {
        public void Configure(EntityTypeBuilder<Permissions> builder)
        {
            // 表名
            builder.ToTable("Permissions");

            // 主键
            builder.HasKey(p => p.Id);

            // Id 自增（INT 类型）
            builder.Property(p => p.Id)
                .UseIdentityColumn();

            // PermissionCode：权限标识，必填，最大长度 50，唯一索引（同一个权限码不能重复）
            builder.Property(p => p.PermissionCode)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            builder.HasIndex(p => p.PermissionCode).IsUnique();

            // PermissionName：权限名称，必填，最大长度 50
            builder.Property(p => p.PermissionName)
                .IsRequired()
                .HasMaxLength(50);

            // PermissionModule：所属模块，必填，最大长度 30（便于按模块分组查询权限）
            builder.Property(p => p.PermissionModule)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false);
            builder.HasIndex(p => p.PermissionModule);
        }
    }
}
