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
        Task<AuthResponse> GoogleLoginAsync(string idToken, CancellationToken ct = default);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        Task RevokeTokenAsync(Guid userId, CancellationToken ct = default);
    }
}
