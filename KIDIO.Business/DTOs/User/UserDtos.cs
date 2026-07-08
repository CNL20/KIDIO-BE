using System;

namespace KIDIO.Business.DTOs.User;

public record SetParentPinRequest(Guid UserId, string NewPin);
public record VerifyPasswordRequest(Guid UserId, string Password);

public record AdminUserResponse(
    Guid Id,
    string DisplayName,
    string Email,
    string Role,
    bool IsEmailConfirmed,
    int ChildrenCount,
    DateTime CreatedAt
);
