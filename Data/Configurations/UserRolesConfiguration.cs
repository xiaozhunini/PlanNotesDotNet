using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 用户-角色关联表配置（复合主键、外键约束、表名映射）
    /// 对应数据库表：user_roles
    /// </summary>
    public class UserRolesConfiguration : IEntityTypeConfiguration<UserRoles>
    {
        public void Configure(EntityTypeBuilder<UserRoles> builder)
        {
            // 显式映射数据库表名（模型名 UserRoles 是驼峰，数据库表名是下划线 user_roles）
            builder.ToTable("user_roles");

            // 复合主键：UserID + RoleID（EF Core 复合主键只能用 Fluent API 配置，不能用数据注解）
            // 保证同一用户不能重复绑定同一角色
            builder.HasKey(ur => new { ur.UserID, ur.RoleID });

            // ======== 外键：UserID → Users.Id ========
            builder.HasOne(ur => ur.User)
                .WithMany()                     // Users 端暂时不声明反向导航集合
                .HasForeignKey(ur => ur.UserID)
                .HasConstraintName("FK_user_roles_users_UserID")
                .OnDelete(DeleteBehavior.Cascade);  // 删除用户时，同步删除其所有角色绑定，避免孤立记录

            // ======== 外键：RoleID → Roles.Id ========
            builder.HasOne(ur => ur.Role)
                .WithMany()                     // Roles 端暂时不声明反向导航集合
                .HasForeignKey(ur => ur.RoleID)
                .HasConstraintName("FK_user_roles_roles_RoleID")
                .OnDelete(DeleteBehavior.Cascade);  // 删除角色时，同步移除所有用户对该角色的绑定

            // CreatedAt：绑定时间，默认当前时间（GETDATE）
            builder.Property(ur => ur.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // RoleID 因为是复合主键的第一列，数据库自动已有索引，无需额外建立
            // 单独建立 UserID 索引（便于按用户查询其所有角色，但其实复合主键最左前缀已覆盖）
        }
    }
}
