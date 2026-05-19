using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static KIDIO.Business.DTOs.Auth.AuthDtos;
using System.Security.Claims;

namespace KIDIO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Đăng nhập bằng Google ID token (từ Google Sign-In SDK phía client)
        /// </summary>
        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> GoogleLogin(
            [FromBody] GoogleLoginRequest request, CancellationToken ct)
        {
            var result = await _authService.GoogleLoginAsync(request.IdToken, ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
        }

        /// <summary>
        /// Dùng refresh token để lấy access token mới
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(
            [FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken, ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result));
        }

        /// <summary>
        /// Đăng xuất — thu hồi refresh token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException());

            await _authService.RevokeTokenAsync(userId, ct);
            return Ok(ApiResponse<object>.Ok(null, "Logged out successfully."));
        }

        /// <summary>
        /// Lấy thông tin user đang đăng nhập
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public ActionResult<ApiResponse<object>> Me()
        {
            var claims = new
            {
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Email = User.FindFirstValue(ClaimTypes.Email),
                Name = User.FindFirstValue(ClaimTypes.Name),
                Role = User.FindFirstValue(ClaimTypes.Role)
            };
            return Ok(ApiResponse<object>.Ok(claims));
        }
    }
}
