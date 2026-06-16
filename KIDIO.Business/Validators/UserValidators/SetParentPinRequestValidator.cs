using FluentValidation;
using KIDIO.Business.DTOs.User;

namespace KIDIO.Business.Validators.UserValidators;

public class SetParentPinRequestValidator : AbstractValidator<SetParentPinRequest>
{
    public SetParentPinRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.NewPin)
            .NotEmpty().WithMessage("PIN cannot be empty.")
            .Length(4).WithMessage("PIN must be exactly 4 digits.")
            .Matches(@"^\d{4}$").WithMessage("PIN must contain only numbers.");
    }
}
