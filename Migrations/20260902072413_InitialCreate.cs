using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanNotesServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PermissionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PermissionModule = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RolesName = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OpenId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    UnionId = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: true),
                    NickName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AvatarUrl = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    UserStatus = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    LastLoginTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshToken = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    PermissionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.RoleID, x.PermissionID });
                    table.ForeignKey(
                        name: "FK_role_permissions_permissions_PermissionID",
                        column: x => x.PermissionID,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "daily_reflections",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    PlanDate = table.Column<DateTime>(type: "DATE", nullable: false),
                    MorningReview = table.Column<string>(type: "TEXT", nullable: true),
                    AfternoonReview = table.Column<string>(type: "TEXT", nullable: true),
                    EveningSummary = table.Column<string>(type: "TEXT", nullable: true),
                    TotalScoreAvg = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_reflections", x => x.ID);
                    table.CheckConstraint("CK_daily_reflections_TotalScoreAvg", "TotalScoreAvg IS NULL OR TotalScoreAvg BETWEEN 0 AND 10");
                    table.ForeignKey(
                        name: "FK_daily_reflections_users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "daily_schedules",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    PlanDate = table.Column<DateTime>(type: "DATE", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    TaskContent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SelfScore = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_schedules", x => x.ID);
                    table.CheckConstraint("CK_daily_schedules_SelfScore", "SelfScore IS NULL OR SelfScore BETWEEN 1 AND 10");
                    table.CheckConstraint("CK_daily_schedules_SortOrder", "SortOrder >= 0");
                    table.CheckConstraint("CK_daily_schedules_StartBeforeEnd", "StartTime < EndTime");
                    table.ForeignKey(
                        name: "FK_daily_schedules_users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_credentials",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    Username = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    LastLoginIP = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_credentials", x => x.ID);
                    table.ForeignKey(
                        name: "FK_user_credentials_users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.UserID, x.RoleID });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "yearly_goals",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TargetYear = table.Column<int>(type: "int", nullable: false),
                    Progress = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    YearlyStatus = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_yearly_goals", x => x.ID);
                    table.CheckConstraint("CK_yearly_goals_Progress", "Progress >= 0 AND Progress <= 100");
                    table.CheckConstraint("CK_yearly_goals_YearlyStatus", "YearlyStatus IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_yearly_goals_users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "weekly_plans",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    YearlyGoalID = table.Column<long>(type: "bigint", nullable: true),
                    WeekStartDate = table.Column<DateTime>(type: "DATE", nullable: false),
                    WeekEndDate = table.Column<DateTime>(type: "DATE", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Review = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_plans", x => x.ID);
                    table.CheckConstraint("CK_weekly_plans_StartBeforeEnd", "WeekStartDate < WeekEndDate");
                    table.ForeignKey(
                        name: "FK_weekly_plans_users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_weekly_plans_yearly_goals_YearlyGoalID",
                        column: x => x.YearlyGoalID,
                        principalTable: "yearly_goals",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_reflections_PlanDate",
                table: "daily_reflections",
                column: "PlanDate");

            migrationBuilder.CreateIndex(
                name: "IX_daily_reflections_TotalScoreAvg",
                table: "daily_reflections",
                column: "TotalScoreAvg");

            migrationBuilder.CreateIndex(
                name: "UQ_daily_reflections_UserID_PlanDate",
                table: "daily_reflections",
                columns: new[] { "UserID", "PlanDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_daily_schedules_CreatedAt",
                table: "daily_schedules",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_daily_schedules_UserID_PlanDate_IsCompleted",
                table: "daily_schedules",
                columns: new[] { "UserID", "PlanDate", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_daily_schedules_UserID_PlanDate_SortOrder",
                table: "daily_schedules",
                columns: new[] { "UserID", "PlanDate", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionCode",
                table: "Permissions",
                column: "PermissionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionModule",
                table: "Permissions",
                column: "PermissionModule");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_PermissionID",
                table: "role_permissions",
                column: "PermissionID");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreatedAt",
                table: "Roles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RolesName",
                table: "Roles",
                column: "RolesName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_credentials_UserID_Unique",
                table: "user_credentials",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_credentials_Username_Unique",
                table: "user_credentials",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleID",
                table: "user_roles",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OpenId",
                table: "Users",
                column: "OpenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UnionId",
                table: "Users",
                column: "UnionId");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_plans_CreatedAt",
                table: "weekly_plans",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_plans_UserID_WeekEndDate",
                table: "weekly_plans",
                columns: new[] { "UserID", "WeekEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_weekly_plans_YearlyGoalID",
                table: "weekly_plans",
                column: "YearlyGoalID");

            migrationBuilder.CreateIndex(
                name: "UQ_weekly_plans_UserID_WeekStartDate",
                table: "weekly_plans",
                columns: new[] { "UserID", "WeekStartDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_yearly_goals_CreatedAt",
                table: "yearly_goals",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_yearly_goals_TargetYear",
                table: "yearly_goals",
                column: "TargetYear");

            migrationBuilder.CreateIndex(
                name: "IX_yearly_goals_UserID_TargetYear",
                table: "yearly_goals",
                columns: new[] { "UserID", "TargetYear" });

            migrationBuilder.CreateIndex(
                name: "IX_yearly_goals_UserID_YearlyStatus",
                table: "yearly_goals",
                columns: new[] { "UserID", "YearlyStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_reflections");

            migrationBuilder.DropTable(
                name: "daily_schedules");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "user_credentials");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "weekly_plans");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "yearly_goals");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
