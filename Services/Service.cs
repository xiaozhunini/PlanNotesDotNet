using AutoMapper;
using PlanNoteServer.DTOs;
using PlanNoteServer.Models;
using PlanNoteServer.Repositories;
using PlanNoteServer.Services.IServices;

namespace PlanNoteServer.Services
{
    public class Service<TEntity, TDto, TCreateDto, TUpdateDto> : IService<TEntity, TDto, TCreateDto, TUpdateDto>
        where TEntity : BaseEntity
        where TDto : class
        where TCreateDto : class
        where TUpdateDto : class
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly IMapper _mapper;

        public Service(IRepository<TEntity> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<TDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<TDto>(entity);
        }

        public async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<TDto>>(entities);
        }

        public async Task<PagedResult<TDto>> GetPagedAsync(int pageIndex, int pageSize)
        {
            var (items, totalCount) = await _repository.GetPagedAsync(pageIndex, pageSize);
            return new PagedResult<TDto>
            {
                Items = _mapper.Map<IEnumerable<TDto>>(items),
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        public async Task<TDto> CreateAsync(TCreateDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<TDto>(created);
        }

        public async Task<TDto?> UpdateAsync(int id, TUpdateDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);
            var updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<TDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return await _repository.ExistsAsync(predicate);
        }
    }
}