using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PlanNoteServer.Data;
using PlanNoteServer.DTOs;
using PlanNoteServer.Models;
using PlanNoteServer.Services.IServices;

namespace PlanNoteServer.Services
{
    /// <summary>
    /// 目标标签业务服务实现类
    /// </summary>
    public class GoalTagsService : IGoalTagsService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// 构造函数：注入数据库上下文和 AutoMapper
        /// </summary>
        public GoalTagsService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// 新增一个目标标签
        /// 业务约束：同一创建者(UserID)下标签名不能重复（对应数据库唯一索引 UQ_goal_tag_UserID_Name）
        /// </summary>
        /// <param name="request">标签创建请求</param>
        /// <returns>创建成功后的标签 DTO</returns>
        public async Task<GoalTagDto> CreateAsync(CreateGoalTagRequest request)
        {
            // 幂等校验：同一创建者下是否已存在同名标签
            var exists = await _context.GoalTags
                .AnyAsync(g => g.UserID == request.UserID && g.Name == request.Name);
            if (exists)
            {
                throw new InvalidOperationException($"标签名称「{request.Name}」已存在");
            }

            var tag = new GoalTags
            {
                Name = request.Name.Trim(),
                Icon = request.Icon?.Trim(),
                Color = request.Color?.Trim(),
                UserID = request.UserID,
                SortOrder = request.SortOrder
                // CreatedAt 由数据库默认值 GETDATE() 自动填充
            };

            _context.GoalTags.Add(tag);
            await _context.SaveChangesAsync();

            return _mapper.Map<GoalTagDto>(tag);
        }

        /// <summary>
        /// 查询当前用户可见的标签列表：系统通用标签（UserID=0）+ 该用户自建标签
        /// 排序：SortOrder 升序（值越小越靠前），同排序按 ID 升序保证稳定
        /// </summary>
        public async Task<IEnumerable<GoalTagDto>> GetUserVisibleTagsAsync(long userId)
        {
            var tags = await _context.GoalTags
                .Where(g => g.UserID == 0 || g.UserID == userId)
                .OrderBy(g => g.SortOrder)
                .ThenBy(g => g.ID)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GoalTagDto>>(tags);
        }
    }
}
