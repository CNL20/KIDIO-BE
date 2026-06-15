using System;
using System.Threading;
using System.Threading.Tasks;
using KIDIO.Business.DTOs.User;

namespace KIDIO.Business.Interfaces;

public interface IUserService
{
    Task SetParentPinAsync(SetParentPinRequest request, CancellationToken ct = default);
    Task<bool> VerifyPasswordAsync(VerifyPasswordRequest request, CancellationToken ct = default);
}
