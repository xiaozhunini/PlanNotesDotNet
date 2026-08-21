using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlanNoteServer.Data.Configurations
{
    /// <summary>
    /// 实体基类配置
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            // 主键配置
            builder.HasKey(e => e.Id);

            // Id 自增
            builder.Property(e => e.Id)
                .UseIdentityColumn();

            // 创建时间
            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // 更新时间
            builder.Property(e => e.UpdatedAt)
                .IsRequired(false);

            // 软删除标记
            builder.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // 全局查询过滤器（软删除）
            builder.HasQueryFilter(e => !e.IsDeleted);

            // 创建时间索引（常用查询字段）
            builder.HasIndex(e => e.CreatedAt);
        }
    }
}
