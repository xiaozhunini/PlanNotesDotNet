namespace PlanNoteServer.Models
{
    /// <summary>
    /// 角色实体（系统字典表，定义 admin / editor / user 等角色）
    /// </summary>
    public class Roles
    {
        /// <summary>
        /// 角色ID（INT，主键，自增）
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 角色标识（如 admin / editor / user，用于代码中的权限判断）
        /// </summary>
        public string RolesName { get; set; } = string.Empty;

        /// <summary>
        /// 角色显示名称（用于界面展示，如“管理员”/“编辑”/“普通用户”）
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 角色描述（补充说明该角色的权限范围）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
