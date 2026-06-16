using System;

namespace KIDIO.Business.DTOs.User;

public record SetParentPinRequest(Guid UserId, string NewPin);
public record VerifyPasswordRequest(Guid UserId, string Password);
