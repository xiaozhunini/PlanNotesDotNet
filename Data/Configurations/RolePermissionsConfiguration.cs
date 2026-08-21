using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 角色-权限关联表配置（复合主键、外键约束、表名映射）
    /// </summary>
    public class RolePermissionsConfiguration : IEntityTypeConfiguration<RolePermissions>
    {
        public void Configure(EntityTypeBuilder<RolePermissions> builder)
        {
            // 显式映射数据库表名（模型名 RolePermissions 是驼峰，数据库表名是下划线 role_permissions）
            builder.ToTable("role_permissions");

            // 复合主键：RoleID + PermissionID（EF Core 复合主键只能用 Fluent API 配置，不能用数据注解）
            builder.HasKey(rp => new { rp.RoleID, rp.PermissionID });

            // ======== 外键：RoleID → Roles.Id  ========
            builder.HasOne(rp => rp.Role)
                .WithMany()                     // Roles 端暂时不声明反向导航集合
                .HasForeignKey(rp => rp.RoleID)
                .HasConstraintName("FK_role_permissions_roles_RoleID")
                .OnDelete(DeleteBehavior.Cascade);  // 删除角色时，同步删除该角色所有权限绑定，避免孤立记录

            // ======== 外键：PermissionID → Permissions.Id  ========
            builder.HasOne(rp => rp.Permission)
                .WithMany()                     // Permissions 端暂时不声明反向导航集合
                .HasForeignKey(rp => rp.PermissionID)
                .HasConstraintName("FK_role_permissions_permissions_PermissionID")
                .OnDelete(DeleteBehavior.Cascade);  // 删除权限时，同步移除所有角色对该权限的绑定

            // 单独建立 PermissionID 索引（便于按权限查询哪些角色拥有它）
            builder.HasIndex(rp => rp.PermissionID)
                .HasDatabaseName("IX_role_permissions_PermissionID");

            // RoleID 因为是复合主键的第一列，数据库自动已有索引，无需额外建立
        }
    }
}
