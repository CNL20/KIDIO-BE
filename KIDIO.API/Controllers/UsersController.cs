using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using FluentValidation;
using KIDIO.Business.DTOs.User;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KIDIO.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<SetParentPinRequest> _setPinValidator;
    private readonly IValidator<VerifyPasswordRequest> _verifyPasswordValidator;

    public UsersController(
        IUserService userService,
        IValidator<SetParentPinRequest> setPinValidator,
        IValidator<VerifyPasswordRequest> verifyPasswordValidator)
    {
        _userService = userService;
        _setPinValidator = setPinValidator;
        _verifyPasswordValidator = verifyPasswordValidator;
    }

    [HttpPost("parent-pin")]
    [HttpPut("parent-pin")]
    public async Task<ActionResult<ApiResponse<object>>> SetParentPin(
        [FromBody] SetParentPinRequest request, CancellationToken ct)
    {
        var validationResult = await _setPinValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
            throw new AppException(firstError ?? "Invalid PIN data.");
        }

        await _userService.SetParentPinAsync(request, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Parental PIN saved successfully."));
    }

    [HttpPost("verify-password")]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyPassword(
        [FromBody] VerifyPasswordRequest request, CancellationToken ct)
    {
        var validationResult = await _verifyPasswordValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault();
            throw new AppException(firstError ?? "Invalid verification request.");
        }

        var result = await _userService.VerifyPasswordAsync(request, ct);
        return Ok(ApiResponse<bool>.Ok(result, result ? "Password verification succeeded." : "Incorrect password."));
    }

    [HttpGet("admin/paged")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResponse<AdminUserResponse>>>> GetAdminUsersPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] string? role = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _userService.GetAdminUsersPagedAsync(pageNumber, pageSize, keyword, role, status, ct);
        return Ok(ApiResponse<PagedResponse<AdminUserResponse>>.Ok(result));
    }

    [HttpPut("admin/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminUserResponse>>> EditUser(
        Guid id, [FromBody] EditUserRequest request, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = Guid.TryParse(userIdString, out Guid currentUserId);

        var result = await _userService.EditUserAsync(id, request, currentUserId, ct);
        return Ok(ApiResponse<AdminUserResponse>.Ok(result, "User updated successfully."));
    }

    [HttpPatch("admin/{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateUserStatus(
        Guid id, [FromBody] UpdateUserStatusRequest request, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = Guid.TryParse(userIdString, out Guid currentUserId);

        await _userService.UpdateUserStatusAsync(id, request, currentUserId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "User status updated successfully."));
    }
}

