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
        int pageNumber = 1, int pageSize = 10, string? keyword = null, string? role = null, string? status = null, CancellationToken ct = default)
    {
        var query = _uow.Users.Query().Include(u => u.Children).AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.ToLower().Trim();
            query = query.Where(u => u.DisplayName.ToLower().Contains(kw) || u.Email.ToLower().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            if (Enum.TryParse<KIDIO.Common.Enums.UserRole>(role, true, out var roleEnum))
            {
                query = query.Where(u => u.Role == roleEnum);
            }
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var stat = status.ToLower().Trim();
            if (stat == "active") query = query.Where(u => !u.IsSuspended);
            else if (stat == "suspended") query = query.Where(u => u.IsSuspended);
            else if (stat == "verified") query = query.Where(u => u.IsEmailConfirmed);
            else if (stat == "unverified") query = query.Where(u => !u.IsEmailConfirmed);
        }

        var mappedQuery = query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserResponse(
                u.Id,
                u.DisplayName,
                u.Email,
                u.Role.ToString(),
                u.IsEmailConfirmed,
                u.IsSuspended,
                u.Children.Count,
                u.CreatedAt
            ));

        return await mappedQuery.ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<AdminUserResponse> EditUserAsync(Guid id, EditUserRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        var user = await _uow.Users.Query().Include(u => u.Children).FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException("User");

        if (!Enum.TryParse<KIDIO.Common.Enums.UserRole>(request.Role, true, out var newRole))
        {
            throw new AppException("Invalid role specified.");
        }

        if (user.Role == KIDIO.Common.Enums.UserRole.Admin && newRole != KIDIO.Common.Enums.UserRole.Admin)
        {
            // Trying to demote an admin. Check if it's the last one.
            var adminCount = await _uow.Users.Query().CountAsync(u => u.Role == KIDIO.Common.Enums.UserRole.Admin && !u.IsSuspended, ct);
            if (adminCount <= 1)
            {
                throw new AppException("Cannot change the role of the last active administrator.");
            }
        }

        user.Role = newRole;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return new AdminUserResponse(
            user.Id,
            user.DisplayName,
            user.Email,
            user.Role.ToString(),
            user.IsEmailConfirmed,
            user.IsSuspended,
            user.Children.Count,
            user.CreatedAt
        );
    }

    public async Task UpdateUserStatusAsync(Guid id, UpdateUserStatusRequest request, Guid currentUserId, CancellationToken ct = default)
    {
        if (id == currentUserId)
        {
            throw new AppException("You cannot change your own suspension status.");
        }

        var user = await _uow.Users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("User");

        user.IsSuspended = request.IsSuspended;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
    }
}
