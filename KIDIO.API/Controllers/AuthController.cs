using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static KIDIO.Business.DTOs.Auth.AuthDtos;
using System.Security.Claims;
using FluentValidation;

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
        private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
        private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
        private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;

        // Inject tất cả Validator qua Constructor
        public AuthController(
            IAuthService authService,
            IConfiguration configuration,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator,
            IValidator<ResendVerificationRequest> resendValidator,
            IValidator<GoogleLoginRequest> googleLoginValidator,
            IValidator<RefreshTokenRequest> refreshValidator,
            IValidator<ChangePasswordRequest> changePasswordValidator,
            IValidator<ForgotPasswordRequest> forgotPasswordValidator,
            IValidator<ResetPasswordRequest> resetPasswordValidator)
        {
            _authService = authService;
            _configuration = configuration;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _resendValidator = resendValidator;
            _googleLoginValidator = googleLoginValidator;
            _refreshValidator = refreshValidator;
            _changePasswordValidator = changePasswordValidator;
            _forgotPasswordValidator = forgotPasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
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
        public async Task<ActionResult<ApiResponse<object>>> VerifyEmail([FromQuery] string token, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new AppException("Verification failed: Token missing!");
            }

            await _authService.VerifyEmailAsync(token, ct);
            
            return Ok(ApiResponse<object>.Ok(null!, "Your KIDIO account has been successfully activated!"));
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
            return Ok(ApiResponse<object>.Ok(null!, "Verification email resent."));
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
            return Ok(ApiResponse<object>.Ok(null!, "Logged out successfully."));
        }

        /// <summary>
        /// Lấy thông tin user đang đăng nhập
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        // [DESIGN FIX #1] Đổi từ object ẩn danh sang UserInfoDto để Swagger sinh schema chính xác
        // và các client có thể deserialize đúng kiểu dữ liệu.
        public ActionResult<ApiResponse<UserInfoDto>> Me()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.FindFirstValue(ClaimTypes.Name);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (id is null || email is null || name is null || role is null)
                throw new UnauthorizedAccessException("Invalid token claims.");

            var userInfo = new UserInfoDto(
                Id: Guid.Parse(id),
                Email: email,
                DisplayName: name,
                AvatarUrl: null, // AvatarUrl không có trong JWT claims; client lấy từ AuthResponse lúc login
                Role: role
            );

            return Ok(ApiResponse<UserInfoDto>.Ok(userInfo));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
            [FromBody] ChangePasswordRequest request, CancellationToken ct)
        {
            var validationResult = await _changePasswordValidator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                throw new AppException(firstError ?? "Invalid data.");
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException());

            await _authService.ChangePasswordAsync(userId, request, ct);
            return Ok(ApiResponse<object>.Ok(null!, "Password changed successfully."));
        }

        [HttpPost("forgot-password")] 
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(
            [FromBody] ForgotPasswordRequest request, CancellationToken ct)
        {
            var validationResult = await _forgotPasswordValidator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                throw new AppException(firstError ?? "Invalid data.");
            }

            await _authService.ForgotPasswordAsync(request.Email, ct);
            return Ok(ApiResponse<object>.Ok(null!, "If the email is registered, a password reset link has been sent."));
        }


        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
            [FromBody] ResetPasswordRequest request, CancellationToken ct)
        {
            var validationResult = await _resetPasswordValidator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var firstError = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
                throw new AppException(firstError ?? "Invalid data.");
            }

            await _authService.ResetPasswordAsync(request, ct);
            return Ok(ApiResponse<object>.Ok(null!, "Password reset successfully. You can now login with your new password."));
        }
    }
}