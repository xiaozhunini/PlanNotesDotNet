using System.Linq.Expressions;
using PlanNoteServer.Models;

namespace PlanNoteServer.Repositories
{
    /// <summary>
    /// 通用仓储接口
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        Task<T?> GetByIdAsync(long id);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// 根据条件查询
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 分页查询
        /// </summary>
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

        /// <summary>
        /// 添加实体
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// 批量添加
        /// </summary>
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// 更新实体
        /// </summary>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// 删除实体（软删除）
        /// </summary>
        Task<bool> DeleteAsync(long id);

        /// <summary>
        /// 物理删除
        /// </summary>
        Task<bool> HardDeleteAsync(long id);

        /// <summary>
        /// 检查是否存在
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 获取数量
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}
