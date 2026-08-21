namespace PlanNoteServer.DTOs
{
    /// <summary>
    /// DTO 基类接口（主键ID为 long 类型，与实体基类保持一致）
    /// </summary>
    public interface IBaseDto
    {
        long Id { get; set; }
    }
}