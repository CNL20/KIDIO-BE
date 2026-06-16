using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using KIDIO.Business.DTOs.Auth;

namespace KIDIO.Business.Validators.AuthValidators
{
    // 3. Validator cho việc gửi lại Email xác thực (Resend)
    public class ResendVerificationRequestValidator : AbstractValidator<AuthDtos.ResendVerificationRequest>
    {
        public ResendVerificationRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("The email address cannot be left blank.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}
