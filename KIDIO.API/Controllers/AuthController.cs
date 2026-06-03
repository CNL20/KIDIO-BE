using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static KIDIO.Business.DTOs.Auth.AuthDtos;
using System.Security.Claims;
using FluentValidation; // Thêm thư viện FluentValidation

namespace KIDIO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Khai báo các Validator cho từng loại Request
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<ResendVerificationRequest> _resendValidator;
        private readonly IValidator<GoogleLoginRequest> _googleLoginValidator;
        private readonly IValidator<RefreshTokenRequest> _refreshValidator;

        // Inject tất cả Validator qua Constructor
        public AuthController(
            IAuthService authService,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator,
            IValidator<ResendVerificationRequest> resendValidator,
            IValidator<GoogleLoginRequest> googleLoginValidator,
            IValidator<RefreshTokenRequest> refreshValidator)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _resendValidator = resendValidator;
            _googleLoginValidator = googleLoginValidator;
            _refreshValidator = refreshValidator;
        }

        /// <summary>
        /// Đăng ký tài khoản mới bằng Email và Mật khẩu
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register(
            [FromBody] RegisterRequest request, CancellationToken ct)
        {
            // Thực hiện Validate dữ liệu đầu vào
            var validationResult = await _registerValidator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                throw new AppException(firstError ?? "Dữ liệu đăng ký không hợp lệ.");
            }

            var result = await _authService.RegisterAsync(request, ct);
            return Ok(ApiResponse<RegisterResponse>.Ok(result, result.Message));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
            [FromBody] LoginRequest request, CancellationToken ct)
        {
            // Thực hiện Validate dữ liệu đầu vào
            var validationResult = await _loginValidator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                throw new AppException(firstError ?? "Dữ liệu đăng nhập không hợp lệ.");
            }

            var result = await _authService.LoginAsync(request, ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
        }

        /// <summary>
        /// Xác thực tài khoản khi người dùng click vào link từ Email
        /// </summary>
        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> VerifyEmail(
            [FromQuery] string token, CancellationToken ct)
        {
            // Token dạng query string thô chỉ cần check đơn giản nếu trống
            if (string.IsNullOrEmpty(token))
            {
                throw new AppException("Verification token is required.");
            }

            await _authService.VerifyEmailAsync(token, ct);
            return Ok(ApiResponse<string>.Ok("Email verified successfully! You can now log in."));
        }

        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> ResendVerification(
            [FromBody] ResendVerificationRequest request, CancellationToken ct)
        {
            // Thực hiện Validate dữ liệu đầu vào
            var validationResult = await _resendValidator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                throw new AppException(firstError ?? "Email không hợp lệ.");
            }

            await _authService.ResendVerificationEmailAsync(request.Email, ct);
            return Ok(ApiResponse<object>.Ok(null, "Verification email resent."));
        }

        /// <summary>
        /// Đăng nhập bằng Google ID token (từ Google Sign-In SDK phía client)
        /// </summary>
        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> GoogleLogin(
            [FromBody] GoogleLoginRequest request, CancellationToken ct)
        {
            // Thực hiện Validate dữ liệu đầu vào
            var validationResult = await _googleLoginValidator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                throw new AppException(firstError ?? "Google IdToken không hợp lệ.");
            }

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
            // Thực hiện Validate dữ liệu đầu vào
            var validationResult = await _refreshValidator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                throw new AppException(firstError ?? "Refresh Token không hợp lệ.");
            }

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