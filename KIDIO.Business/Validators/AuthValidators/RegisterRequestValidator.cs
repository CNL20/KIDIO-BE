using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using KIDIO.Business.DTOs.Auth;

namespace KIDIO.Business.Validators.AuthValidators
{
    // 1. Validator cho Đăng ký tài khoản (Register)
    public class RegisterRequestValidator : AbstractValidator<AuthDtos.RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("The email address cannot be left blank.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(100).WithMessage("Email addresses must not exceed 100 characters.");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("The display name cannot be left blank.")
                .MinimumLength(2).WithMessage("The display name must have at least 2 characters.")
                .MaximumLength(50).WithMessage("The display name must not exceed 50 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("The password cannot be left blank.")
                .MinimumLength(8).WithMessage("The password must have at least 8 characters.")
                .Matches(@"[A-Z]").WithMessage("The password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("The password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("The password must contain at least one digit.")
                .Matches(@"[\^$*.\[\]{}()?""!@#%&/\\,><':;|_~`]").WithMessage("The password must contain at least one special character.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Please re-enter your confirmation password.")
                .Equal(x => x.Password).WithMessage("The re-entered password does not match the previously entered password.");
        }
    }
}
