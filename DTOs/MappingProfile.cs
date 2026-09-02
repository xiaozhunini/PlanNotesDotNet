
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
        }
    }
}
