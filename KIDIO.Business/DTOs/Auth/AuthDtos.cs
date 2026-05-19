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

        //Response
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
