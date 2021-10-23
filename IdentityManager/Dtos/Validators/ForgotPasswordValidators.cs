using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace IdentityManager.Dtos.Validators
{
    public class ForgotPasswordValidators : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordValidators()
        {
            RuleFor(x => x.Email).EmailAddress().WithMessage("Email format is not valid")
                                .NotNull().WithMessage("Email field is required");
        }
    }
}