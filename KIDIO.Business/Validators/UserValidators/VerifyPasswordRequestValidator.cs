using FluentValidation;
using KIDIO.Business.DTOs.User;

namespace KIDIO.Business.Validators.UserValidators;

public class VerifyPasswordRequestValidator : AbstractValidator<VerifyPasswordRequest>
{
    public VerifyPasswordRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be empty.");
    }
}
