namespace PlanNoteServer.Services.IServices
{
    /// <summary>
    /// 目标标签业务服务接口
    /// </summary>
    public interface IGoalTagsService
    {
        /// <summary>
        /// 新增一个目标标签
        /// </summary>
        /// <param name="request">标签创建请求</param>
        /// <returns>创建成功后的标签 DTO</returns>
        Task<DTOs.GoalTagDto> CreateAsync(DTOs.CreateGoalTagRequest request);

        /// <summary>
        /// 查询当前用户可见的标签列表：系统通用标签（UserID=0）+ 该用户自建标签
        /// </summary>
        /// <param name="userId">当前用户 ID</param>
        /// <returns>用户可见的标签集合（按 SortOrder 升序，同排序按 ID 升序）</returns>
        Task<IEnumerable<DTOs.GoalTagDto>> GetUserVisibleTagsAsync(long userId);
    }
}
