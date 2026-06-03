using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using KIDIO.Business.DTOs.Auth;

namespace KIDIO.Business.Validators.AuthValidators
{
    // 2. Validator cho Đăng nhập truyền thống (Login)
    public class LoginRequestValidator : AbstractValidator<AuthDtos.LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Định dạng email không hợp lệ.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.");
        }
    }
}
