using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlanNoteServer.Data;
using PlanNoteServer.DTOs;
using PlanNoteServer.Services.IServices;

namespace PlanNoteServer.Services
{
    /// <summary>
    /// 年度目标业务服务实现类
    /// </summary>
    public class YearlyGoalsService : IYearlyGoalsService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// 构造函数：注入数据库上下文（用于 Include 关联标签查询）和 AutoMapper
        /// </summary>
        public YearlyGoalsService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// 查询指定用户在某一年的所有年度目标（含关联标签）
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="year">目标年份</param>
        /// <returns>年度目标列表，含关联标签信息</returns>
        public async Task<IEnumerable<YearlyGoalDto>> GetByUserIdAndYearAsync(long userId, int year)
        {
            var goals = await _context.YearlyGoals
                .Include(g => g.Tag)              // 关联查询标签表 goal_tag
                .Where(g => g.UserID == userId && g.TargetYear == year)
                .OrderByDescending(g => g.CreatedAt)  // 按创建时间倒序，最新的在前
                .ToListAsync();

            return _mapper.Map<IEnumerable<YearlyGoalDto>>(goals);
        }

        /// <summary>
        /// 新增一个年度目标
        /// 业务约束：TagId 非空时必须是存在的标签（系统通用标签或该用户自建标签）
        /// </summary>
        public async Task<YearlyGoalDto> CreateAsync(long userId, CreateYearlyGoalRequest request)
        {
            // 校验绑定的标签存在且当前用户可见
            if (request.TagId.HasValue)
            {
                var tagExists = await _context.GoalTags
                    .AnyAsync(t => t.ID == request.TagId.Value &&
                                   (t.UserID == 0 || t.UserID == userId));
                if (!tagExists)
                {
                    throw new InvalidOperationException("绑定的标签不存在或无权使用");
                }
            }

            var goal = new Models.YearlyGoals
            {
                UserID = userId,
                TagId = request.TagId,
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                TargetYear = request.TargetYear
                // Progress 默认 0、YearlyStatus 默认 1（进行中）由实体默认值承担
                // CreatedAt 由数据库默认值 GETDATE() 自动填充
            };

            _context.YearlyGoals.Add(goal);
            await _context.SaveChangesAsync();

            // 重新加载关联标签，保证返回值含完整标签信息
            await _context.Entry(goal).Reference(g => g.Tag).LoadAsync();

            return _mapper.Map<YearlyGoalDto>(goal);
        }
    }
}
