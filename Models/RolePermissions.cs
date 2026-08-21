namespace PlanNoteServer.Models
{
    /// <summary>
    /// 角色-权限关联表（RBAC：一个角色可以拥有多个权限）
    /// 对应数据库表：role_permissions
    /// 主键为组合主键（RoleID + PermissionID），保证同一角色不能重复绑定同一权限
    /// </summary>
    public class RolePermissions
    {
        /// <summary>
        /// 关联角色ID（INT，复合主键之一，外键 → Roles.Id）
        /// </summary>
        public int RoleID { get; set; }

        /// <summary>
        /// 关联权限ID（INT，复合主键之一，外键 → Permissions.Id）
        /// </summary>
        public int PermissionID { get; set; }

        // 导航属性（可选，便于 EF 从任一方向查询关联对象）

        /// <summary>
        /// 导航属性：关联的角色（查询时 Include 可拿到角色详情）
        /// </summary>
        public Roles? Role { get; set; }

        /// <summary>
        /// 导航属性：关联的权限（查询时 Include 可拿到权限详情）
        /// </summary>
        public Permissions? Permission { get; set; }
    }
}
