using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityManager.Dtos.Validators
{
    public class LoginValidators : AbstractValidator<LoginDto>
    {
        public LoginValidators()
        {
            RuleFor(x => x.Email).EmailAddress().WithMessage("Email format not valid")
                                .NotNull();
            RuleFor(x => x.Password).NotNull();
        }
    }
}