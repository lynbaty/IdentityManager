using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityManager.Dtos
{
    public class TwoFactorAuthenticator
    {
        public string Code { get; set; }
        public string Token { set; get; }
    }
}
