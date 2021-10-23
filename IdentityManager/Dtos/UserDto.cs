using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityManager.Dtos
{
    public class UserDto
    {
        public string Id { set; get; }
        [Required]
        public string Name { set; get; }
        public string Email { set; get; }
        public List<SelectListItem> RoleList { set; get; }
        public string RoleSelected { set; get; }
    }
}
