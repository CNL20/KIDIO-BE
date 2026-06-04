using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static KIDIO.Business.DTOs.Auth.AuthDtos;
using System.Security.Claims;
using FluentValidation; // Thêm thư viện FluentValidation
using System.IO; // Thêm thư viện để đọc file HTML template

namespace KIDIO.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        // Khai báo các Validator cho từng loại Request
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<ResendVerificationRequest> _resendValidator;
        private readonly IValidator<GoogleLoginRequest> _googleLoginValidator;
        private readonly IValidator<RefreshTokenRequest> _refreshValidator;

        // Inject tất cả Validator qua Constructor
        public AuthController(
            IAuthService authService,
            IConfiguration configuration,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator,
            IValidator<ResendVerificationRequest> resendValidator,
            IValidator<GoogleLoginRequest> googleLoginValidator,
            IValidator<RefreshTokenRequest> refreshValidator)
        {
            _authService = authService;
            _configuration = configuration;
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
                throw new AppException(firstError ?? "The registration data is invalid.");
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
                throw new AppException(firstError ?? "Invalid login data.");
            }

            var result = await _authService.LoginAsync(request, ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
        }

        /// <summary>
        /// Xác thực tài khoản khi người dùng click vào link từ Email
        /// </summary>
        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token, CancellationToken ct)
        {
            // Do file đã được cấu hình tự động copy ra thư mục output khi Build, 
            // nên tầng API vẫn đọc trực tiếp từ thư mục chạy "Templates" cực kỳ dễ dàng:
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "VerifyLink.html");

            if (!System.IO.File.Exists(templatePath))
            {
                // Đoạn này để phòng hờ nếu bạn quên chỉnh "Copy if newer" khiến file không tìm thấy
                return StatusCode(500, "The system is missing the configuration file for the trading interface.");
            }

            string htmlContent = await System.IO.File.ReadAllTextAsync(templatePath, ct);

            if (string.IsNullOrWhiteSpace(token))
            {
                htmlContent = htmlContent
                    .Replace("{{StatusContent}}", "<h2 class='error'>Verification failed: Token missing!</h2>")
                    .Replace("{{Status}}", "failed");
                return Content(htmlContent, "text/html");
            }

            try
            {
                await _authService.VerifyEmailAsync(token, ct);

                htmlContent = htmlContent
                    .Replace("{{StatusContent}}", "<h2 class='success'>Your KIDIO account has been successfully activated!</h2>")
                    .Replace("{{Status}}", "success");
            }
            catch (AppException)
            {
                htmlContent = htmlContent
                    .Replace("{{StatusContent}}", "<h2 class='error'>The verification link is invalid or has expired!</h2>")
                    .Replace("{{Status}}", "invalid");
            }
            catch
            {
                htmlContent = htmlContent
                    .Replace("{{StatusContent}}", "<h2 class='error'>The system is busy, please try again later!</h2>")
                    .Replace("{{Status}}", "server_error");
            }

            return Content(htmlContent, "text/html");
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
                throw new AppException(firstError ?? "Invalid email address.");
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
                throw new AppException(firstError ?? "The Google ID Token is invalid.");
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
                throw new AppException(firstError ?? "The Refresh Token is invalid.");
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