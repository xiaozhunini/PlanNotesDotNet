using System.Text;
using PlanNoteServer.Configuration;
using PlanNoteServer.Data;
using PlanNoteServer.DTOs;
using PlanNoteServer.Middleware;
using PlanNoteServer.Repositories;
using PlanNoteServer.Services;
using PlanNoteServer.Services.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// 配置数据库上下文
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 配置JWT设置
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// 配置 Redis 设置
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));

// 注册 Redis 连接（单例：IConnectionMultiplexer 是线程安全的长连接，全局共享一个实例）
var redisSettings = builder.Configuration.GetSection("Redis").Get<RedisSettings>() ?? new RedisSettings();
var redisConfig = new ConfigurationOptions
{
    EndPoints = { redisSettings.ConnectionString },
    AbortOnConnectFail = redisSettings.AbortOnConnectFail,
    Ssl = redisSettings.Ssl,
    DefaultDatabase = redisSettings.DefaultDatabase,
    ConnectRetry = 3,
    ConnectTimeout = 5000,
    SyncTimeout = 5000
};
var redis = ConnectionMultiplexer.Connect(redisConfig);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddScoped<ITokenStore, RedisTokenStore>();

// 注册AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 注册仓储
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// 注册密码哈希器
builder.Services.AddScoped<IPasswordHasher<PlanNoteServer.Models.Users>, PasswordHasher<PlanNoteServer.Models.Users>>();

// 注册服务
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 配置JWT认证
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings?.SecretKey ?? string.Empty);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers["Token-Expired"] = "true";
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var message = "未授权访问";
            if (context.Error != null)
            {
                switch (context.Error)
                {
                    case "invalid_token":
                        message = "无效的令牌";
                        break;
                    case "token_expired":
                        message = "令牌已过期";
                        break;
                    case "invalid_signature":
                        message = "令牌签名无效";
                        break;
                    default:
                        message = "令牌验证失败: " + context.Error;
                        break;
                }
            }
            var result = System.Text.Json.JsonSerializer.Serialize(new { StatusCode = 401, Message = message });
            return context.Response.WriteAsync(result);
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

// 配置Swagger支持JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlanNoteServer API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// 初始化数据库种子数据（自动迁移 + 灌入 RBAC 基础数据和管理员账号；幂等：已有则跳过）
await DbSeeder.SeedAsync(app.Services);

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();