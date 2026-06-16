using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIDIO.Business.DTOs.Auth
{
    public class AuthDtos
    {
        //Request
        public record GoogleLoginRequest(string IdToken);
        public record RefreshTokenRequest(string RefreshToken);
        public record LoginRequest(string Email, string Password);
        public record ResendVerificationRequest(string Email);
        public record RegisterRequest(
            string Email,
            string Password,
            string ConfirmPassword, 
            string DisplayName
        );
        public record ChangePasswordRequest(
            string OldPassword, 
            string NewPassword, 
            string ConfirmNewPassword
        );
        public record ForgotPasswordRequest(string Email);
        public record ResetPasswordRequest(
            string Token, 
            string NewPassword, 
            string ConfirmNewPassword
        );
        //Response
        public record RegisterResponse(
            Guid Id,
            string Email,
            string Message 
        );
        public record AuthResponse(
            string AccessToken,
            string RefreshToken,
            DateTime AccessTokenExpiry,
            UserInfoDto User
        );

        public record UserInfoDto(
            Guid Id,
            string Email,
            string DisplayName,
            string? AvatarUrl,
            string Role
        );

    }
}
