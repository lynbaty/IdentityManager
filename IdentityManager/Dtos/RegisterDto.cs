using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityManager.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string Email { get; set; }

        public string Password { set; get; }

        public string ConfirmPassword { set; get; }
        public string Name { set; get; }

        public List<SelectListItem> RoleList { set; get; }

        public string RoleSelected { set; get; }
    }
}