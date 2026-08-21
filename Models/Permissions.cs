namespace PlanNoteServer.Models
{
    /// <summary>
    /// 权限实体（RBAC 权限字典，定义系统所有可授权的操作）
    /// 权限标识 PermissionCode 命名格式：{模块}:{操作}  如 news:publish / activity:audit
    /// </summary>
    public class Permissions
    {
        /// <summary>
        /// 权限ID（INT，主键，自增）
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 权限标识（如 news:publish、activity:audit，全局唯一，用于代码中鉴权判断）
        /// </summary>
        public string PermissionCode { get; set; } = string.Empty;

        /// <summary>
        /// 权限名称（用于界面展示，如“发布文章”、“审核活动”）
        /// </summary>
        public string PermissionName { get; set; } = string.Empty;

        /// <summary>
        /// 所属模块（归类权限所属业务模块，如 News / Activity / User / Plan / Note）
        /// </summary>
        public string PermissionModule { get; set; } = string.Empty;
    }
}
