using System;
using System.Threading;
using System.Threading.Tasks;
using KIDIO.Business.DTOs.User;
using KIDIO.Common;

namespace KIDIO.Business.Interfaces;

public interface IUserService
{
    Task SetParentPinAsync(SetParentPinRequest request, CancellationToken ct = default);
    Task<bool> VerifyPasswordAsync(VerifyPasswordRequest request, CancellationToken ct = default);
    Task<PagedResponse<AdminUserResponse>> GetAdminUsersPagedAsync(int pageNumber = 1, int pageSize = 10, string? keyword = null, CancellationToken ct = default);
}
