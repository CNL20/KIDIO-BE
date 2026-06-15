using System;
using System.Threading;
using System.Threading.Tasks;
using KIDIO.Business.DTOs.User;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Repositories;

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
}
