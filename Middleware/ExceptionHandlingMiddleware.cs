using System.Net;
using System.Text.Json;

namespace PlanNoteServer.Middleware
{
    /// <summary>
    /// 全局异常处理中间件
    /// 用于捕获整个请求管道中未被处理的异常，统一返回标准化的 JSON 错误响应，防止系统崩溃或暴露敏感信息
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// 构造函数，注入下一个中间件委托和日志记录器
        /// </summary>
        /// <param name="next">请求管道中的下一个中间件</param>
        /// <param name="logger">日志记录器，用于记录异常信息</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// 中间件的核心执行方法
        /// </summary>
        /// <param name="context">当前 HTTP 请求的上下文</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // 尝试执行管道中的下一个中间件（最终会执行到 Controller）
                await _next(context);
            }
            catch (Exception ex)
            {
                // 如果后续流程中抛出了任何未被捕获的异常，则进入异常处理逻辑
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// 处理异常并构建统一的 JSON 响应
        /// </summary>
        /// <param name="context">当前 HTTP 请求的上下文</param>
        /// <param name="exception">捕获到的异常对象</param>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // 1. 将异常记录到日志中，方便后续排查问题
            _logger.LogError(exception, "未处理的异常");

            // 2. 设置响应的内容类型为 JSON
            context.Response.ContentType = "application/json";

            // 3. 定义默认的响应结构（默认返回 500 服务器内部错误）
            var response = new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "服务器内部错误",
                Detail = exception.Message
            };

            // 4. 根据异常的具体类型，返回不同的 HTTP 状态码和提示信息
            switch (exception)
            {
                case UnauthorizedAccessException:
                    // 未授权访问异常 -> 401
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized,
                        Message = "未授权访问",
                        Detail = exception.Message
                    };
                    break;
                case KeyNotFoundException:
                    // 资源未找到异常 -> 404
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "资源未找到",
                        Detail = exception.Message
                    };
                    break;
                case ArgumentException:
                case InvalidOperationException:
                    // 参数无效或无效操作异常 -> 400
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = "请求参数无效",
                        Detail = exception.Message
                    };
                    break;
                default:
                    // 其他未知异常 -> 500
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            // 5. 将响应对象序列化为 JSON 字符串并写入 HTTP 响应流
            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}