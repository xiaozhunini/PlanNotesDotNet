using AutoMapper;
using PlanNoteServer.DTOs;
using PlanNoteServer.Models;
using PlanNoteServer.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PlanNoteServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public abstract class BaseController<TEntity, TDto, TCreateDto, TUpdateDto> : ControllerBase
        where TEntity : BaseEntity
        where TDto : class, IBaseDto
        where TCreateDto : class
        where TUpdateDto : class
    {
        protected readonly IService<TEntity, TDto, TCreateDto, TUpdateDto> _service;
        protected readonly ILogger<BaseController<TEntity, TDto, TCreateDto, TUpdateDto>> _logger;

        public BaseController(
            IService<TEntity, TDto, TCreateDto, TUpdateDto> service,
            ILogger<BaseController<TEntity, TDto, TCreateDto, TUpdateDto>> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<TDto>>> GetAll()
        {
            try
            {
                var items = await _service.GetAllAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取数据失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TDto>> GetById(int id)
        {
            try
            {
                var item = await _service.GetByIdAsync(id);
                if (item == null)
                    return NotFound($"ID为{id}的数据不存在");

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取数据失败，ID: {Id}", id);
                return StatusCode(500, "服务器内部错误");
            }
        }

        [HttpGet("paged")]
        public virtual async Task<ActionResult<PagedResult<TDto>>> GetPaged(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetPagedAsync(pageIndex, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分页查询失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        [HttpPost]
        public virtual async Task<ActionResult<TDto>> Create([FromBody] TCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建数据失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        [HttpPut("{id}")]
        public virtual async Task<ActionResult<TDto>> Update(int id, [FromBody] TUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updated = await _service.UpdateAsync(id, dto);
                if (updated == null)
                    return NotFound($"ID为{id}的数据不存在");

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新数据失败，ID: {Id}", id);
                return StatusCode(500, "服务器内部错误");
            }
        }

        [HttpDelete("{id}")]
        public virtual async Task<ActionResult<bool>> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                    return NotFound($"ID为{id}的数据不存在");

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除数据失败，ID: {Id}", id);
                return StatusCode(500, "服务器内部错误");
            }
        }
    }
}