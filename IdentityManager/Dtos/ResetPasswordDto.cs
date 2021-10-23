using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityManager.Dtos
{
    public class ResetPasswordDto
    {
        public string NewPassword { set; get; }

        public string ConfirmPassword { set; get; }

        public string Code { get; set; }

        public string UserId {set; get;}
        
    }
}