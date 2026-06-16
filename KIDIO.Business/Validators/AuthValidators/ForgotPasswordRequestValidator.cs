using FluentValidation;
using static KIDIO.Business.DTOs.Auth.AuthDtos;

namespace KIDIO.Business.Validators.AuthValidators
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");
        }
    }
}
