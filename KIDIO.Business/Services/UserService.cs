using System;
using System.Threading;
using System.Threading.Tasks;
using KIDIO.Business.DTOs.User;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Repositories;
using KIDIO.Business.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace KIDIO.Business.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;

    public UserService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task SetParentPinAsync(SetParentPinRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User");

        user.ParentalPin = request.NewPin;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<bool> VerifyPasswordAsync(VerifyPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User");

        // OAuth accounts (e.g. Google, Facebook) won't have a PasswordHash in DB
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
    }

    public async Task<PagedResponse<AdminUserResponse>> GetAdminUsersPagedAsync(
        int pageNumber = 1, int pageSize = 10, string? keyword = null, CancellationToken ct = default)
    {
        var query = _uow.Users.Query().Include(u => u.Children).AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.ToLower().Trim();
            query = query.Where(u => u.DisplayName.ToLower().Contains(kw) || u.Email.ToLower().Contains(kw));
        }

        var mappedQuery = query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserResponse(
                u.Id,
                u.DisplayName,
                u.Email,
                u.Role.ToString(),
                u.IsEmailConfirmed,
                u.Children.Count,
                u.CreatedAt
            ));

        return await mappedQuery.ToPagedResponseAsync(pageNumber, pageSize, ct);
    }
}
