namespace PlanNoteServer.Services.IServices
{
    /// <summary>
    /// 年度目标业务服务接口
    /// </summary>
    public interface IYearlyGoalsService
    {
        /// <summary>
        /// 查询指定用户在某一年的所有年度目标（含关联标签）
        /// </summary>
        /// <param name="userId">用户 ID</param>
        /// <param name="year">目标年份（如：2024）</param>
        /// <returns>该用户该年份下的全部年度目标列表，含标签信息；无数据返回空集合</returns>
        Task<IEnumerable<DTOs.YearlyGoalDto>> GetByUserIdAndYearAsync(long userId, int year);

        /// <summary>
        /// 新增一个年度目标（绑定标签，进度默认 0，状态默认进行中）
        /// </summary>
        /// <param name="userId">当前用户 ID（从 JWT 解析）</param>
        /// <param name="request">目标创建请求</param>
        /// <returns>创建成功后的年度目标 DTO（含标签信息）</returns>
        Task<DTOs.YearlyGoalDto> CreateAsync(long userId, DTOs.CreateYearlyGoalRequest request);
    }
}
