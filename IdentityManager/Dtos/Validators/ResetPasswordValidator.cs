using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace IdentityManager.Dtos.Validators
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.NewPassword).NotNull()
                                .MaximumLength(100).MinimumLength(6).WithMessage("Password is longer 6 character");
            RuleFor(x => x.ConfirmPassword).NotNull()
                                .Equal(register => register.NewPassword).WithMessage("Password is not match");
        }   
    }
}