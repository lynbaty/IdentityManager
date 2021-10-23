using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityManager.Dtos
{
    public class UserClaimDto
    {
        public string UserId { set; get; }
        public List<UserClaim> Claims { set; get; } = new List<UserClaim>();
    }
    public class UserClaim
    {
        public string ClaimType { set; get; }
        public bool IsSelected { set; get; }
    }
        

}
