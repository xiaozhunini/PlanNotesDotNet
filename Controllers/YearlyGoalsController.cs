using System.Security.Claims;
using PlanNoteServer.DTOs;
using PlanNoteServer.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PlanNoteServer.Controllers
{
    /// <summary>
    /// 年度目标控制器（提供年计划查询等接口）
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 年计划属于个人数据，必须登录后访问
    public class YearlyGoalsController : ControllerBase
    {
        private readonly IYearlyGoalsService _yearlyGoalsService;
        private readonly ILogger<YearlyGoalsController> _logger;

        /// <summary>
        /// 构造函数，通过依赖注入获取年度目标服务和日志记录器
        /// </summary>
        public YearlyGoalsController(IYearlyGoalsService yearlyGoalsService, ILogger<YearlyGoalsController> logger)
        {
            _yearlyGoalsService = yearlyGoalsService;
            _logger = logger;
        }

        /// <summary>
        /// 查询指定用户在某一年的所有年度目标（含关联标签信息）
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="year">目标年份（如：2024）</param>
        /// <returns>该用户该年份下的全部年度目标列表，含标签信息</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DTOs.YearlyGoalDto>>> GetByUserIdAndYear(
            [FromQuery] long userId,
            [FromQuery] int year)
        {
            try
            {
                // 参数校验
                if (userId <= 0)
                    return BadRequest(new { message = "userId 必须大于 0" });
                if (year < 1970 || year > 2100)
                    return BadRequest(new { message = "year 年份范围必须在 1970 ~ 2100 之间" });

                var goals = await _yearlyGoalsService.GetByUserIdAndYearAsync(userId, year);
                return Ok(goals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询年度目标失败：userId={UserId}, year={Year}", userId, year);
                return StatusCode(500, new { message = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 新增一个年度目标（可绑定标签 ID）
        /// UserID 从 JWT 令牌中解析，无需前端传入；进度默认 0，状态默认进行中
        /// </summary>
        /// <param name="request">目标创建请求（Title、TargetYear 必填，TagId/Description 可选）</param>
        /// <returns>创建成功后的年度目标信息（含标签）</returns>
        [HttpPost]
        public async Task<ActionResult<YearlyGoalDto>> Create([FromBody] CreateYearlyGoalRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // 从 JWT Claims 中解析当前用户 ID
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!long.TryParse(userIdStr, out var userId) || userId <= 0)
                {
                    return Unauthorized(new { message = "无法识别用户身份，请重新登录" });
                }

                var goal = await _yearlyGoalsService.CreateAsync(userId, request);
                return CreatedAtAction(nameof(Create), new { id = goal.ID }, goal);
            }
            catch (InvalidOperationException ex)
            {
                // 业务异常（如标签不存在）返回 400
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增年度目标失败：{Title}", request.Title);
                return StatusCode(500, new { message = "服务器内部错误" });
            }
        }
    }
}
