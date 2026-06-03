using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KIDIO.Business.DTOs.Auth.AuthDtos;

namespace KIDIO.Business.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<AuthResponse> GoogleLoginAsync(string idToken, CancellationToken ct = default);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        Task RevokeTokenAsync(Guid userId, CancellationToken ct = default);
        Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default);
        Task ResendVerificationEmailAsync(string email, CancellationToken ct = default);

    }
}
