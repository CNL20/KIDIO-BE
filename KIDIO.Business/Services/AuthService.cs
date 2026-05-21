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
using Microsoft.SqlServer.Server;
using static KIDIO.Business.DTOs.Auth.AuthDtos;

namespace KIDIO.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly JwtSettings _jwt;
        private readonly GoogleOAuthSettings _google;
        private readonly AdminSettings _admin;

        public AuthService(
        IUnitOfWork uow,
        IOptions<JwtSettings> jwt,
        IOptions<GoogleOAuthSettings> google,
        IOptions<AdminSettings> admin)
        {
            _uow = uow;
            _jwt = jwt.Value;
            _google = google.Value;
            _admin = admin.Value;
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

    }
}
