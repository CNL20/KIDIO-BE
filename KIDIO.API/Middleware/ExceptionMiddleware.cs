using System.Net;
using System.Text.Json;
using KIDIO.Common;

namespace KIDIO.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            // 1. Đưa các Exception cụ thể (Thằng con) lên TRÊN CÙNG
            catch (UnauthorizedException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
                await WriteResponse(context, ex.Message);
            }
            catch (NotFoundException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                await WriteResponse(context, ex.Message);
            }
            // 2. Đưa Exception chung (Thằng cha) xuống DƯỚI
            catch (AppException ex)
            {
                context.Response.StatusCode = ex.StatusCode; // 400
                await WriteResponse(context, ex.Message);
            }
            // 3. Lỗi hệ thống nghiêm trọng luôn nằm cuối cùng
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
                await WriteResponse(context, "An unexpected error occurred.");
            }
        }

        private static async Task WriteResponse(HttpContext context, string message)
        {
            context.Response.ContentType = "application/json";
            var response = ApiResponse<object>.Fail(message);
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await context.Response.WriteAsync(json);
        }

    }
}
