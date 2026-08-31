using PlanNoteServer.Models;
using Microsoft.EntityFrameworkCore;

namespace PlanNoteServer.Data
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        #region 实体集合定义
        /// <summary>
        /// 用户实体集合
        /// </summary>
        public DbSet<Users> Users { get; set; }

        /// <summary>
        /// 角色实体集合（系统字典：admin / editor / user）
        /// </summary>
        public DbSet<Roles> Roles { get; set; }

        /// <summary>
        /// 权限实体集合（RBAC 权限字典：PermissionCode 格式 {模块}:{操作}）
        /// </summary>
        public DbSet<Permissions> Permissions { get; set; }

        /// <summary>
        /// 角色-权限关联集合（RBAC：一个角色绑定哪些权限，对应表 role_permissions）
        /// </summary>
        public DbSet<RolePermissions> RolePermissions { get; set; }

        /// <summary>
        /// 用户-角色关联集合（RBAC：一个用户绑定哪些角色，对应表 user_roles）
        /// </summary>
        public DbSet<UserRoles> UserRoles { get; set; }

        /// <summary>
        /// 用户登录凭证集合（账号密码登录体系，对应表 user_credentials，可选绑定 1:1 Users）
        /// </summary>
        public DbSet<UserCredentials> UserCredentials { get; set; }

        /// <summary>
        /// 年度目标集合（OKR 年度规划，对应表 yearly_goals）
        /// </summary>
        public DbSet<YearlyGoals> YearlyGoals { get; set; }

        /// <summary>
        /// 周计划集合（对应表 weekly_plans，可与年度目标可选关联）
        /// </summary>
        public DbSet<WeeklyPlans> WeeklyPlans { get; set; }

        /// <summary>
        /// 每日日程集合（对应表 daily_schedules，按天细分的打卡时间块）
        /// </summary>
        public DbSet<DailySchedules> DailySchedules { get; set; }

        /// <summary>
        /// 每日复盘集合（对应表 daily_reflections，一日三阶段反思 + 当日平均得分，1用户1天1条）
        /// </summary>
        public DbSet<DailyReflections> DailyReflections { get; set; }
        #endregion


        /// <summary>
        /// 配置实体映射（自动扫描程序集中所有实现了 IEntityTypeConfiguration<T> 的配置类，从而将实体的字段约束、索引、表名等配置应用到数据库）
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 应用所有配置类
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        #region 自动维护时间戳设置

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }
        
        /// <summary>
        /// UpdateTimestamps （方法作用是每次进行事务的时候添加操作），ChangeTracker  变更追踪器
        /// </summary>
        private void UpdateTimestamps()
        { 
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.Now;
                }
            }
        }
        #endregion
    }
}
