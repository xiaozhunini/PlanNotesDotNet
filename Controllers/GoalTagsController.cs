using System.Security.Claims;
using PlanNoteServer.DTOs;
using PlanNoteServer.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PlanNoteServer.Controllers
{
    /// <summary>
    /// 目标标签控制器（标签的新增等接口）
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 标签操作必须登录后访问
    public class GoalTagsController : ControllerBase
    {
        private readonly IGoalTagsService _goalTagsService;
        private readonly ILogger<GoalTagsController> _logger;

        /// <summary>
        /// 构造函数，通过依赖注入获取标签服务和日志记录器
        /// </summary>
        public GoalTagsController(IGoalTagsService goalTagsService, ILogger<GoalTagsController> logger)
        {
            _goalTagsService = goalTagsService;
            _logger = logger;
        }

        /// <summary>
        /// 新增一个目标标签
        /// </summary>
        /// <param name="request">标签创建请求（Name 必填，Icon 为网络链接，Color 为色值，UserID/SortOrder 可选）</param>
        /// <returns>创建成功后的标签信息</returns>
        [HttpPost]
        public async Task<ActionResult<GoalTagDto>> Create([FromBody] CreateGoalTagRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tag = await _goalTagsService.CreateAsync(request);
                // 返回 201 Created
                return CreatedAtAction(nameof(Create), new { id = tag.ID }, tag);
            }
            catch (InvalidOperationException ex)
            {
                // 业务异常（如重名）返回 400
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增标签失败：{Name}", request.Name);
                return StatusCode(500, new { message = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 查询当前用户可见的标签列表（系统通用标签 + 自己创建的标签）
        /// user_id 从 JWT 令牌中解析，无需前端传入
        /// </summary>
        /// <returns>标签列表（按 SortOrder 升序排列）</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GoalTagDto>>> GetMyTags()
        {
            try
            {
                // 从 JWT Claims 中解析当前用户 ID（与 /api/auth/me 接口一致）
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!long.TryParse(userIdStr, out var userId) || userId <= 0)
                {
                    return Unauthorized(new { message = "无法识别用户身份，请重新登录" });
                }

                var tags = await _goalTagsService.GetUserVisibleTagsAsync(userId);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询标签列表失败");
                return StatusCode(500, new { message = "服务器内部错误" });
            }
        }
    }
}
