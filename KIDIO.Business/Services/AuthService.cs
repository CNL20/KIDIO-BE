using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth;
using KIDIO.Business.DTOs.Auth;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Common.Enums;
using KIDIO.Data.Entities;
using KIDIO.Data.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
// Removed unused Microsoft.SqlServer.Server import (project uses PostgreSQL)
using static KIDIO.Business.DTOs.Auth.AuthDtos;

namespace KIDIO.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly JwtSettings _jwt;
        private readonly GoogleOAuthSettings _google;
        private readonly AdminSettings _admin;
        private readonly IEmailService _emailService;
        private readonly UrlSettings _urlSettings;

        public AuthService(
         IUnitOfWork uow,
         IOptions<JwtSettings> jwt,
         IOptions<GoogleOAuthSettings> google,
         IOptions<AdminSettings> admin,
         IEmailService emailService,
         IOptions<UrlSettings> urlSettings) 
        {
            _uow = uow;
            _jwt = jwt.Value;
            _google = google.Value;
            _admin = admin.Value;
            _emailService = emailService;
            _urlSettings = urlSettings.Value;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            // 2. Kiểm tra Email này đã tồn tại trong hệ thống chưa
            var existingUser = await _uow.Users.FirstOrDefaultAsync(
                u => u.Email.ToLower() == request.Email.ToLower(), ct);

            if (existingUser is not null)
            {
                throw new AppException("This email address has already been used.");
            }

            // 3. Mã hóa mật khẩu (Sử dụng BCrypt để băm mật khẩu bảo mật)
            // Lưu ý: Đảm bảo thực thể 'User' của bạn có thuộc tính 'PasswordHash' (hoặc tương tự)
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var verificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", "").Replace("+", "").Replace("/", "");
            // 4. Tạo đối tượng User mới
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                DisplayName = request.DisplayName,
                PasswordHash = passwordHash, // Gán mật khẩu đã mã hóa
                Role = UserRole.Parent,      // Định nghĩa role mặc định phù hợp với dự án KIDIO (ví dụ: Phụ huynh)
                AvatarUrl = null,            // Tài khoản mới tạo chưa có avatar
                GoogleId = null,

                IsEmailConfirmed = false, // Chưa kích hoạt
                EmailVerificationToken = verificationToken,
                EmailVerificationTokenExpiryTime = DateTime.UtcNow.AddHours(24)
            };

            // 5. Lưu vào Database qua Unit of Work
            await _uow.Users.AddAsync(newUser, ct);
            await _uow.SaveChangesAsync(ct);

            // 6. Gửi email xác nhận
            // Bọc trong try/catch: tài khoản đã tạo thành công trong DB.
            // Nếu email gửi thất bại (VD: chưa cấu hình SMTP ở môi trường dev), 
            // không rollback tài khoản mà trả thông báo hướng dẫn resend.
            string verificationMessage;
            try
            {
                string confirmationLink = $"{_urlSettings.FrontendUrl}/verify-email?token={verificationToken}";
                string emailBody = await LoadEmailTemplateWithLinkAsync(confirmationLink);
                await _emailService.SendEmailAsync(newUser.Email, "KIDIO - Confirm Your Email", emailBody);
                verificationMessage = "Account registration successful! Please check your email to verify your account before logging in.";
            }
            catch (Exception ex)
            {
                // Email sending failed but the account is created successfully in DB.
                // Do not block registration. Allow user to request resend later.
                verificationMessage = "Account created successfully, but the verification email could not be sent. " +
                                      $"Details: {ex.Message}. Please contact the administrator.";
            }

            // 7. Trả về thông báo thành công để FE chuyển hướng sang trang Login
            return new RegisterResponse(
                Id: newUser.Id,
                Email: newUser.Email,
                Message: verificationMessage
            );
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            // Tìm user theo email
            var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), ct)
                       ?? throw new UnauthorizedException("Incorrect email or password.");

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new UnauthorizedException("This account was created using Google Sign-In. Please sign in with Google.");
            }

            // [FIX #2] Kiểm tra xác thực email TRƯỚC khi verify mật khẩu để tránh side-channel attack.
            // Nếu check sau verify password, kẻ tấn công biết được mật khẩu đúng/sai qua loại lỗi trả về.
            if (!user.IsEmailConfirmed)
            {
                throw new UnauthorizedException("Your account has not been verified via email yet.");
            }

            // Kiểm tra mật khẩu (Sử dụng BCrypt để verify mật khẩu thô và mật khẩu đã băm trong DB)
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValidPassword)
            {
                throw new UnauthorizedException("Incorrect email or password.");
            }

            // Cấp Token giống hệt bên GoogleLogin
            var (accessToken, expiry) = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);

            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);

            return BuildAuthResponse(accessToken, expiry, refreshToken, user);
        }
        public async Task<AuthResponse> GoogleLoginAsync(string idToken, CancellationToken ct = default)
        {
            // 1. Verify Google ID token
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _google.ClientId }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (InvalidJwtException)
            {
                throw new UnauthorizedException("Invalid Google token.");
            }

            var isAdminEmail = _admin.Emails.Any(email =>
                string.Equals(email, payload.Email, StringComparison.OrdinalIgnoreCase));

            // 2. Tìm hoặc tạo user
            var user = await _uow.Users.FirstOrDefaultAsync(
                u => u.GoogleId == payload.Subject || u.Email == payload.Email, ct);

            if (user is null)
            {
                user = new User
                {
                    Email = payload.Email,
                    DisplayName = payload.Name ?? payload.Email,
                    AvatarUrl = payload.Picture,
                    GoogleId = payload.Subject,
                    Role = isAdminEmail ? UserRole.Admin : UserRole.Parent
                };
                await _uow.Users.AddAsync(user, ct);
            }
            else
            {
                if (isAdminEmail && user.Role != UserRole.Admin)
                    user.Role = UserRole.Admin;

                // Cập nhật GoogleId nếu login bằng email lần đầu
                if (user.GoogleId is null)
                    user.GoogleId = payload.Subject;

                // Sync avatar nếu chưa có
                if (user.AvatarUrl is null && payload.Picture is not null)
                    user.AvatarUrl = payload.Picture;

                _uow.Users.Update(user);
            }

            // 3. Issue tokens
            var (accessToken, expiry) = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);

            await _uow.SaveChangesAsync(ct);

            return BuildAuthResponse(accessToken, expiry, refreshToken, user);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            var user = await _uow.Users.FirstOrDefaultAsync(
                u => u.RefreshToken == refreshToken, ct)
                ?? throw new UnauthorizedException("Invalid refresh token.");

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token expired. Please login again.");

            var (accessToken, expiry) = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);

            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);

            return BuildAuthResponse(accessToken, expiry, newRefreshToken, user);
        }

        public async Task RevokeTokenAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct)
                ?? throw new NotFoundException("User");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);
        }

        // ── Helpers ─────────────────────────────────────────────

        private (string token, DateTime expiry) GenerateAccessToken(User user)
        {
            var expiry = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.DisplayName),
                new(ClaimTypes.Role, user.Role.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
        }

        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static AuthResponse BuildAuthResponse(
            string accessToken, DateTime expiry, string refreshToken, User user)
        {
            return new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken,
                AccessTokenExpiry: expiry,
                User: new UserInfoDto(
                    Id: user.Id,
                    Email: user.Email,
                    DisplayName: user.DisplayName,
                    AvatarUrl: user.AvatarUrl,
                    Role: user.Role.ToString()
                )
            );
        }

        public async Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default)
        {
            // Tìm user sở hữu token này
            var user = await _uow.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token, ct)
                       ?? throw new AppException("Invalid or used verification token.");

            // Kiểm tra thời hạn token
            if (user.EmailVerificationTokenExpiryTime < DateTime.UtcNow)
            {
                throw new AppException("Verification token has expired.");
            }

            // Xác thực thành công -> Clear sạch token đi
            user.IsEmailConfirmed = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiryTime = null;

            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);

            return true;
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
        {
            var user = await _uow.Users.GetByIdAsync(userId, ct)
                ?? throw new NotFoundException("User");

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new AppException("This account is managed by Google Sign-In and does not have a local password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            {
                throw new AppException("Incorrect old password.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            
            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
        {
            var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);
            if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            {
                // To prevent email enumeration, we just return
                return;
            }

            var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", "").Replace("+", "").Replace("/", "");

            user.ResetPasswordToken = resetToken;
            user.ResetPasswordTokenExpiryTime = DateTime.UtcNow.AddHours(1);

            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);

            string resetLink = $"{_urlSettings.FrontendUrl}/reset-password?token={resetToken}";
            string emailBody = await LoadEmailTemplateWithLinkAsync(resetLink, "ResetPassword.html");

            try
            {
                await _emailService.SendEmailAsync(user.Email, "KIDIO - Reset Your Password", emailBody);
            }
            catch (AppException)
            {
                // Anti-enumeration: không ré lộ lỗi email ra bên ngoài. Đã log trong EmailService.
                // ForgotPassword không nên throw bất kỳ exception nào dù email gửi thất bại.
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        {
            var user = await _uow.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == request.Token, ct)
                ?? throw new AppException("Invalid or expired reset token.");

            if (user.ResetPasswordTokenExpiryTime < DateTime.UtcNow)
            {
                throw new AppException("Reset token has expired.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiryTime = null;

            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);
        }

        private async Task<string> LoadEmailTemplateWithLinkAsync(string link, string templateName = "VerifyEmail.html")
        {
            // 1. Thiết lập vị trí tệp mẫu nằm trong thư mục gốc chạy của dự án Web API
            string templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", templateName);

            // 2. Cơ chế dự phòng (Fallback) nếu lỡ quên chưa copy file HTML vào thư mục build
            if (!File.Exists(templatePath))
            {
                if (templateName == "VerifyEmail.html")
                    return $"<h3>Welcome to KIDIO!</h3><p>Please verify your email by clicking: <a href='{link}'>Verify Email</a></p>";
                else
                    return $"<h3>KIDIO Password Reset</h3><p>Please reset your password by clicking: <a href='{link}'>Reset Password</a></p>";
            }

            // 3. Đọc toàn bộ chuỗi HTML tĩnh từ tệp tin
            string templateContent = await File.ReadAllTextAsync(templatePath);

            // 4. Gán giá trị link của Frontend vào đúng vị trí {{Link}} trong file HTML
            return templateContent.Replace("{{ConfirmationLink}}", link).Replace("{{ResetLink}}", link);
        }

        public async Task ResendVerificationEmailAsync(string email, CancellationToken ct = default)
        {
            var user = await _uow.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct)
                       ?? throw new NotFoundException("User");

            // [FIX #1] Removed dead code: the `if (user is null)` block was unreachable
            // because the `?? throw` above already guarantees user is non-null at this point.

            if (user.IsEmailConfirmed)
            {
                throw new AppException("Email is already confirmed.");
            }

            var verificationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("=", "").Replace("+", "").Replace("/", "");

            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpiryTime = DateTime.UtcNow.AddHours(24);

            _uow.Users.Update(user);
            await _uow.SaveChangesAsync(ct);

            string confirmationLink = $"{_urlSettings.FrontendUrl}/verify-email?token={verificationToken}";
            string emailBody = await LoadEmailTemplateWithLinkAsync(confirmationLink);

            try
            {
                await _emailService.SendEmailAsync(user.Email, "KIDIO - Confirm Your Email", emailBody);
            }
            catch (AppException ex) when (ex.StatusCode == 503)
            {
                throw new AppException("Verification email could not be sent. Email service is not configured. Please contact the administrator.", 503);
            }
        }
    }
}
