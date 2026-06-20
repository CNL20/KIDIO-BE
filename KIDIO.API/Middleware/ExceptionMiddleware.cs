using System.Net;
using System.Text.Json;
using KIDIO.Common;
using Microsoft.EntityFrameworkCore;

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
            // ── 1. Domain exceptions — từ cụ thể đến tổng quát ──────────────
            catch (ForbiddenException ex)
            {
                // ForbiddenException kế thừa AppException nhưng cần 403, đặt trước AppException
                context.Response.StatusCode = 403;
                await WriteResponse(context, ex.Message);
            }
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
            catch (AppException ex)
            {
                context.Response.StatusCode = ex.StatusCode; // thường là 400
                await WriteResponse(context, ex.Message);
            }

            // ── 2. Lỗi xác thực / token ─────────────────────────────────────
            // UnauthorizedAccessException là System exception (không phải custom), ném khi
            // GetCurrentUserId() không tìm thấy claim trong JWT → trả 401 thay vì 500.
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
                await WriteResponse(context, "Unauthorized: " + ex.Message);
            }

            // ── 3. Lỗi DB constraint (unique index vi phạm, FK) ─────────────
            // Khi EF Core vi phạm unique index (ví dụ duplicate achievement) → 409 Conflict
            // thay vì 500 Internal Server Error.
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Database update constraint violation");
                var inner = ex.InnerException?.Message ?? ex.Message;

                // Kiểm tra xem có phải lỗi unique constraint không
                if (inner.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                    || inner.Contains("unique", StringComparison.OrdinalIgnoreCase)
                    || inner.Contains("23505")) // PostgreSQL unique violation error code
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict; // 409
                    await WriteResponse(context, "A record with the same unique data already exists.");
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                    await WriteResponse(context, "Database error: " + inner);
                }
            }

            // ── 4. Request bị hủy (client ngắt kết nối) ────────────────────
            // Không nên log lỗi 500 cho trường hợp client tự đóng kết nối.
            catch (OperationCanceledException)
            {
                // 499 = Client Closed Request (nginx convention, không có trong HttpStatusCode enum)
                context.Response.StatusCode = 499;
                // Không cần ghi response vì client đã ngắt kết nối
            }

            // ── 5. Lỗi format / argument – thường do input sai ──────────────
            // FormatException xảy ra khi Guid.Parse() nhận chuỗi không hợp lệ.
            // ArgumentException xảy ra khi enum parse thất bại ngoài AppException.
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Format error in request: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                // Không trả ex.Message thô — có thể lộ thông tin nội bộ (VD: JWT config errors)
                await WriteResponse(context, "Invalid format in request data. Please check your input.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in request: {Message}", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                await WriteResponse(context, "Invalid argument in request. Please check your input.");
            }

            // ── 6. Lỗi hệ thống không mong đợi — luôn cuối cùng ────────────
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Type} — {Message}", ex.GetType().Name, ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
                await WriteResponse(context, "An unexpected error occurred. Please try again later.");
            }
        }

        private static async Task WriteResponse(HttpContext context, string message)
        {
            // Bỏ qua nếu response đã bắt đầu ghi (streaming) hoặc client đã ngắt
            if (context.Response.HasStarted) return;

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
