namespace PlanNoteServer.Models
{
    /// <summary>
    /// 用户-角色关联表（RBAC：一个用户可以绑定多个角色，一个角色可赋予多个用户）
    /// 对应数据库表：user_roles
    /// 主键为组合主键（UserID + RoleID），保证同一用户不能重复绑定同一角色
    /// </summary>
    public class UserRoles
    {
        /// <summary>
        /// 关联用户ID（BIGINT，复合主键之一，外键 → Users.Id）
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 关联角色ID（INT，复合主键之一，外键 → Roles.Id）
        /// </summary>
        public int RoleID { get; set; }

        /// <summary>
        /// 绑定时间（可选，方便追溯何时赋予该角色）
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ===== 导航属性（可选，便于 EF 从任一方向查询关联对象）=====

        /// <summary>
        /// 导航属性：关联的用户（查询时 Include 可拿到用户详情）
        /// </summary>
        public Users? User { get; set; }

        /// <summary>
        /// 导航属性：关联的角色（查询时 Include 可拿到角色详情）
        /// </summary>
        public Roles? Role { get; set; }
    }
}
