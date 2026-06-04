using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using KIDIO.Business.DTOs.Auth;

namespace KIDIO.Business.Validators.AuthValidators
{
    // 4. Validator cho Đăng nhập bằng Google
    public class GoogleLoginRequestValidator : AbstractValidator<AuthDtos.GoogleLoginRequest>
    {
        public GoogleLoginRequestValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("The Google ID Token cannot be left blank.");
        }
    }
}
