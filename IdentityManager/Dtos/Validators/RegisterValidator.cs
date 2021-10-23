using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityManager.Dtos.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Email).NotNull().WithMessage("Email field is required")
                                .EmailAddress().WithMessage("Email format is not valid")
                                .WithName("Email");
            RuleFor(x => x.Password).NotNull()
                                .MaximumLength(100).MinimumLength(6).WithMessage("Password is longer 6 character");
            RuleFor(x => x.ConfirmPassword).NotNull()
                                .Equal(register => register.Password).WithMessage("Password is not match");
            RuleFor(x => x.Name).NotNull();
        }
    }
}