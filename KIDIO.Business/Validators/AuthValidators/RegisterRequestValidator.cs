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
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Định dạng email không hợp lệ.")
                .MaximumLength(100).WithMessage("Email không được vượt quá 100 ký tự.");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Tên hiển thị không được để trống.")
                .MinimumLength(2).WithMessage("Tên hiển thị phải có ít nhất 2 ký tự.")
                .MaximumLength(50).WithMessage("Tên hiển thị không được vượt quá 50 ký tự.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .Matches(@"[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ cái viết hoa.")
                .Matches(@"[a-z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ cái viết thường.")
                .Matches(@"[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ số.")
                .Matches(@"[\^$*.\[\]{}()?""!@#%&/\\,><':;|_~`]").WithMessage("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Vui lòng nhập lại mật khẩu xác nhận.")
                .Equal(x => x.Password).WithMessage("Mật khẩu nhập lại không khớp với mật khẩu đã nhập.");
        }
    }
}
