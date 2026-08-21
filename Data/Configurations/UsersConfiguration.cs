using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 微信用户实体配置（字段长度、唯一索引、默认值等约束）
    /// </summary>
    public class UsersConfiguration : BaseEntityConfiguration<Users>
    {
        public override void Configure(EntityTypeBuilder<Users> builder)
        {
            // 先调用基类配置（主键、时间字段、软删除等）
            base.Configure(builder);

            // 表名
            builder.ToTable("Users");

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

            // RoleType：角色类型（1普通用户/2编辑/3管理员），默认 1
            builder.Property(u => u.RoleType)
                .IsRequired()
                .HasDefaultValue((byte)1);

            // UserStatus：账号状态（0正常/1禁用），默认 0
            builder.Property(u => u.UserStatus)
                .IsRequired()
                .HasDefaultValue((byte)0);

            // LastLoginTime：最后登录时间，可选
            builder.Property(u => u.LastLoginTime)
                .IsRequired(false);

            // RefreshToken：刷新令牌，可选，最大长度 256
            builder.Property(u => u.RefreshToken)
                .IsRequired(false)
                .HasMaxLength(256)
                .IsUnicode(false);

            // RefreshTokenExpiryTime：刷新令牌过期时间，可选
            builder.Property(u => u.RefreshTokenExpiryTime)
                .IsRequired(false);
        }
    }
}
