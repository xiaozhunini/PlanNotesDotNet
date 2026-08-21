using PlanNoteServer.DTOs;
using PlanNoteServer.Models;

namespace PlanNoteServer.Services.IServices
{
    public interface IService<TEntity, TDto, TCreateDto, TUpdateDto>
        where TEntity : BaseEntity
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class
    {
        Task<TDto?> GetByIdAsync(int id);

        Task<IEnumerable<TDto>> GetAllAsync();

        Task<PagedResult<TDto>> GetPagedAsync(
            int pageIndex,
            int pageSize);

        Task<TDto> CreateAsync(TCreateDto dto);

        Task<TDto?> UpdateAsync(int id, TUpdateDto dto);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);
    }
}