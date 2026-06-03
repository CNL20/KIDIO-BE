using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using KIDIO.Business.DTOs.Auth;

namespace KIDIO.Business.Validators.AuthValidators
{
    // 5. Validator cho việc làm mới Token (Refresh Token)
    public class RefreshTokenRequestValidator : AbstractValidator<AuthDtos.RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh Token không được để trống.");
        }
    }
}
