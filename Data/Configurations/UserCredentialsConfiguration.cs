using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 用户凭证表配置（字段长度、唯一索引、外键约束、表名映射）
    /// </summary>
    public class UserCredentialsConfiguration : IEntityTypeConfiguration<UserCredentials>
    {
        public void Configure(EntityTypeBuilder<UserCredentials> builder)
        {
            // 显式映射数据库表名（模型驼峰 UserCredentials → 数据库下划线 user_credentials）
            builder.ToTable("user_credentials");

            // 主键：ID（BIGINT 自增）
            builder.HasKey(uc => uc.ID);
            builder.Property(uc => uc.ID)
                .UseIdentityColumn();

            // UserID：关联 users 表（BIGINT，与 Users.Id 类型一致）
            builder.Property(uc => uc.UserID)
                .IsRequired();
            // 1 个用户最多绑定 1 组账号密码，加唯一索引
            builder.HasIndex(uc => uc.UserID)
                .IsUnique()
                .HasDatabaseName("IX_user_credentials_UserID_Unique");

            // Username：登录账号，必填，最大长度 50，唯一索引（登录查询直接走唯一索引最快）
            builder.Property(uc => uc.Username)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            builder.HasIndex(uc => uc.Username)
                .IsUnique()
                .HasDatabaseName("IX_user_credentials_Username_Unique");

            // PasswordHash：bcrypt 密文，必填，最大长度 128
            // bcrypt 默认输出长度约 60 字符，预留至 128 应对升级迭代（如 2b/2y 前缀版本变化）
            builder.Property(uc => uc.PasswordHash)
                .IsRequired()
                .HasMaxLength(128)
                .IsUnicode(false);

            // LastLoginIP：最后登录 IP（IPv4 15 字符 / IPv6 45 字符），可空
            builder.Property(uc => uc.LastLoginIP)
                .IsRequired(false)
                .HasMaxLength(45)
                .IsUnicode(false);

            // ===== 外键：UserID → Users.Id（级联删除：用户被删时凭证一起删，避免孤立记录）=====
            builder.HasOne(uc => uc.User)
                .WithOne()                         // Users 端暂不声明反向导航（1:1 关系，一侧写够）
                .HasForeignKey<UserCredentials>(uc => uc.UserID)
                .HasConstraintName("FK_user_credentials_users_UserID")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
