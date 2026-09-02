using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlanNoteServer.Models;

namespace PlanNoteServer.Data
{
    /// <summary>
    /// 数据库种子数据初始化器（应用启动时调用，幂等：已有数据则跳过）。
    /// 包含 RBAC 基础数据（权限/角色/角色-权限关联）和一个本地账号密码登录的管理员账号。
    /// 顺序：权限 → 角色 → 角色-权限 → 用户 → 凭证 → 用户-角色
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>
        /// 启动时调用：自动迁移 + 按依赖顺序插入种子数据。
        /// </summary>
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var db = provider.GetRequiredService<AppDbContext>();
            var passwordHasher = provider.GetRequiredService<IPasswordHasher<Users>>();

            // 自动应用迁移（确保表结构已生成，开发环境首次启动会建表）
            await db.Database.MigrateAsync();

            await SeedPermissionsAsync(db);
            await SeedRolesAsync(db);
            await SeedRolePermissionsAsync(db);
            await SeedAdminUserAsync(db, passwordHasher);
            await SeedAdminUserRoleAsync(db);
        }

        // ===== 1. 权限种子数据（共 20 条）=====
        private static async Task SeedPermissionsAsync(AppDbContext db)
        {
            if (await db.Permissions.AnyAsync()) return;

            var permissions = new[]
            {
                NewPermission("user:view", "查看用户", "用户管理"),
                NewPermission("user:add", "新增用户", "用户管理"),
                NewPermission("user:edit", "编辑用户", "用户管理"),
                NewPermission("user:delete", "删除用户", "用户管理"),
                NewPermission("user:disable", "禁用/启用用户", "用户管理"),
                NewPermission("role:view", "查看角色", "角色管理"),
                NewPermission("role:add", "新增角色", "角色管理"),
                NewPermission("role:edit", "编辑角色", "角色管理"),
                NewPermission("role:delete", "删除角色", "角色管理"),
                NewPermission("yearly_goal:view", "查看年计划", "年计划"),
                NewPermission("yearly_goal:add", "新增年计划", "年计划"),
                NewPermission("yearly_goal:edit", "编辑年计划", "年计划"),
                NewPermission("yearly_goal:delete", "删除年计划", "年计划"),
                NewPermission("weekly_plan:view", "查看周计划", "周计划"),
                NewPermission("weekly_plan:add", "新增周计划", "周计划"),
                NewPermission("weekly_plan:edit", "编辑周计划", "周计划"),
                NewPermission("weekly_plan:delete", "删除周计划", "周计划"),
                NewPermission("daily_schedule:view", "查看日计划", "日计划"),
                NewPermission("daily_schedule:manage", "管理日计划", "日计划"),
                NewPermission("daily_reflection:manage", "管理日反思", "日反思")
            };

            await db.Permissions.AddRangeAsync(permissions);
            await db.SaveChangesAsync();
        }

        private static Permissions NewPermission(string code, string name, string module) => new()
        {
            PermissionCode = code,
            PermissionName = name,
            PermissionModule = module
        };

        // ===== 2. 角色种子数据（admin / editor / user）=====
        private static async Task SeedRolesAsync(AppDbContext db)
        {
            if (await db.Roles.AnyAsync()) return;

            var roles = new[]
            {
                new Roles { RolesName = "admin",  DisplayName = "超级管理员", Description = "拥有所有权限" },
                new Roles { RolesName = "editor", DisplayName = "编辑人员",  Description = "可管理计划内容，不能管理用户和角色" },
                new Roles { RolesName = "user",   DisplayName = "普通用户",  Description = "只能管理自己的计划数据" }
            };

            await db.Roles.AddRangeAsync(roles);
            await db.SaveChangesAsync();
        }

        // ===== 3. 角色-权限关联 =====
        private static async Task SeedRolePermissionsAsync(AppDbContext db)
        {
            if (await db.RolePermissions.AnyAsync()) return;

            var allPermissions = await db.Permissions.ToListAsync();
            var planModules = new[] { "年计划", "周计划", "日计划", "日反思" };
            var planPermissions = allPermissions
                .Where(p => planModules.Contains(p.PermissionModule))
                .ToList();

            var adminRole = await db.Roles.FirstAsync(r => r.RolesName == "admin");
            var editorRole = await db.Roles.FirstAsync(r => r.RolesName == "editor");
            var userRole = await db.Roles.FirstAsync(r => r.RolesName == "user");

            var rolePermissions = new List<RolePermissions>();

            // admin → 全部权限
            rolePermissions.AddRange(allPermissions.Select(p => new RolePermissions
            {
                RoleID = adminRole.Id,
                PermissionID = p.Id
            }));

            // editor / user → 计划相关权限（业务层用 UserID 做数据隔离）
            foreach (var role in new[] { editorRole, userRole })
            {
                rolePermissions.AddRange(planPermissions.Select(p => new RolePermissions
                {
                    RoleID = role.Id,
                    PermissionID = p.Id
                }));
            }

            await db.RolePermissions.AddRangeAsync(rolePermissions);
            await db.SaveChangesAsync();
        }

        // ===== 4. 管理员用户 + 登录凭证 =====
        private static async Task SeedAdminUserAsync(AppDbContext db, IPasswordHasher<Users> passwordHasher)
        {
            var existingCred = await db.UserCredentials.FirstOrDefaultAsync(c => c.Username == "admin");
            if (existingCred != null) return;

            var adminUser = await db.Users.FirstOrDefaultAsync(u => u.OpenId == "local-admin");

            if (adminUser == null)
            {
                adminUser = new Users
                {
                    OpenId = "local-admin",
                    NickName = "小猪迪迪",
                    AvatarUrl = null,
                    Phone = null,
                    UserStatus = 0,
                    LastLoginTime = null
                };

                await db.Users.AddAsync(adminUser);
                await db.SaveChangesAsync();
            }

            var hash = passwordHasher.HashPassword(adminUser, "Admin@2024");

            await db.UserCredentials.AddAsync(new UserCredentials
            {
                UserID = adminUser.Id,
                Username = "admin",
                PasswordHash = hash,
                LastLoginIP = null
            });
            await db.SaveChangesAsync();
        }

        // ===== 5. 管理员绑定 admin 角色 =====
        private static async Task SeedAdminUserRoleAsync(AppDbContext db)
        {
            var adminUser = await db.Users.FirstOrDefaultAsync(u => u.OpenId == "local-admin");
            var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.RolesName == "admin");
            if (adminUser == null || adminRole == null) return;

            var exists = await db.UserRoles.AnyAsync(ur => ur.UserID == adminUser.Id && ur.RoleID == adminRole.Id);
            if (exists) return;

            await db.UserRoles.AddAsync(new UserRoles
            {
                UserID = adminUser.Id,
                RoleID = adminRole.Id
            });
            await db.SaveChangesAsync();
        }
    }
}
