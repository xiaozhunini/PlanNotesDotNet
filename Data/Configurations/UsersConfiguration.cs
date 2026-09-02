using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 微信用户实体配置（字段长度、唯一索引、默认值、软删除查询过滤器）
    /// </summary>
    public class UsersConfiguration : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            // ===== 原 BaseEntityConfiguration 内联 =====

            // 主键：Id（BIGINT 自增）
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .UseIdentityColumn();

            // 创建时间（默认 GETDATE）
            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // 更新时间（可空，首次更新时自动填充）
            builder.Property(e => e.UpdatedAt)
                .IsRequired(false);

            // 软删除标记（默认 false）
            builder.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // 全局查询过滤器：所有 LINQ 查询自动过滤掉软删除记录
            builder.HasQueryFilter(e => !e.IsDeleted);

            // 创建时间索引
            builder.HasIndex(e => e.CreatedAt);

            // ===== 业务字段配置 =====

            // 表名（数据库统一使用小写下划线命名）
            builder.ToTable("users");

            // OpenId：微信唯一标识，必填，最大长度 64，唯一索引
            builder.Property(u => u.OpenId)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);
            builder.HasIndex(u => u.OpenId).IsUnique();

            // UnionId：微信开放平台 unionid，可选，最大长度 64（多端关联用）
            builder.Property(u => u.UnionId)
                .IsRequired(false)
                .HasMaxLength(64)
                .IsUnicode(false);
            builder.HasIndex(u => u.UnionId);

            // NickName：昵称，必填，最大长度 50
            builder.Property(u => u.NickName)
                .IsRequired()
                .HasMaxLength(50);

            // AvatarUrl：头像地址，可选，最大长度 255
            builder.Property(u => u.AvatarUrl)
                .IsRequired(false)
                .HasMaxLength(255)
                .IsUnicode(false);

            // Phone：手机号（加密存储），最大长度 20
            builder.Property(u => u.Phone)
                .IsRequired(false)
                .HasMaxLength(20)
                .IsUnicode(false);

            // 角色已迁移到 user_roles 关联表，Users 表不再有 RoleType 字段

            // UserStatus：账号状态（0正常/1禁用），默认 0
            builder.Property(u => u.UserStatus)
                .IsRequired()
                .HasDefaultValue((byte)0);

            // LastLoginTime：最后登录时间，可选
            builder.Property(u => u.LastLoginTime)
                .IsRequired(false);

            // RefreshToken / RefreshTokenExpiryTime 已迁移到 Redis（RedisTokenStore），此处不再配置
        }
    }
}
