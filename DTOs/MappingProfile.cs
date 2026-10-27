
using AutoMapper;
using PlanNoteServer.Models;

namespace PlanNoteServer.DTOs
{
    /// <summary>
    /// AutoMapper 映射配置（实体与 DTO 之间的对象映射规则）
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 用户实体映射
            CreateMap<Users, UserDto>();

            // 年度目标实体映射（含关联标签导航属性，自动递归映射 GoalTags → GoalTagDto）
            CreateMap<YearlyGoals, YearlyGoalDto>();

            // 目标标签实体映射
            CreateMap<GoalTags, GoalTagDto>();
        }
    }
}
