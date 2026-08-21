namespace PlanNoteServer.Models
{
    /// <summary>
    /// 实体基类（所有实体的公共字段）
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// 主键ID（BIGINT，自增）
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 创建时间（注册时间）
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 是否删除（软删除标记）
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
