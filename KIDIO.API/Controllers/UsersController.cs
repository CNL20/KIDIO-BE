using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
}
