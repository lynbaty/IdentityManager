using AspNetCoreHero.ToastNotification.Abstractions;
using IdentityManager.Data;
using IdentityManager.Dtos;
using IdentityManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityManager.Controllers
{
    [Authorize(Roles ="Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly INotyfService _notyf;

        public UserController(UserManager<ApplicationUser> userManager,AppDbContext context, INotyfService notyf)
        {
            _userManager = userManager;
            _context = context;
            _notyf = notyf;
        }
        public async Task<IActionResult> Index()
        {
            await UpdateUserRoles();
            var users = _context.ApplicationUsers.ToList();
            return View(users);
        }

        private async Task UpdateUserRoles()
        {
            var users = _context.ApplicationUsers.ToList();
            foreach(var user in users)
            {
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                user.Role = role ?? "None";
                _context.Update(user);
            }
            await _context.SaveChangesAsync();
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            await UpdateUserRoles();
            var roles = _context.Roles.Select(x => new SelectListItem { Text = x.Name, Value = x.Name }).ToList();
            var user = _context.ApplicationUsers.FirstOrDefault(x => x.Id == id);

            var userdto = new UserDto
            {
                Name = user.Name,
                Email = user.Email,
                Id = user.Id,
                RoleList = roles
            };

            return View(userdto);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UserDto request)
        {
            if(request.RoleSelected == null)
            {
                _notyf.Warning("Update Fail");
                var roles = _context.Roles.Select(x => new SelectListItem { Text = x.Name, Value = x.Name }).ToList();
                request.RoleList = roles;
                return View(request);
            }
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(request.Id);
                user.Name = request.Name;
                var recentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                if (recentRole != null) await _userManager.RemoveFromRoleAsync(user, recentRole);
                await _userManager.AddToRoleAsync(user, request.RoleSelected);
                _notyf.Success("Update User Succeeded");
                return RedirectToAction(nameof(Index));
            }
            _notyf.Warning("Update Fail");
            return View(request);

        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = _context.ApplicationUsers.FirstOrDefault(x => x.Id == id);
            if(user == null)
            {
                _notyf.Success("Delete User Failed");
                return RedirectToAction(nameof(Index));
            }
            await _userManager.DeleteAsync(user);
            _notyf.Success("Delete User Succeeded");
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> ManageClaim(string UserId)
        {
            var claimDto = new UserClaimDto()
            {
                UserId = UserId
            };
            var user = await _userManager.FindByIdAsync(UserId);
            var claim = await _userManager.GetClaimsAsync(user);
            foreach (var item in ClaimStore.claimslist)
            {
                var UserClaim = new UserClaim()
                {
                    ClaimType = item.Type
                };
                if (claim != null)
                {
                    if (claim.Any(x => x.Type == item.Type))
                        UserClaim.IsSelected = true;
                }
                claimDto.Claims.Add(UserClaim);

            }
            return View(claimDto);
        }
        [HttpPost]
        public async Task<IActionResult> ManageClaim(UserClaimDto request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            var existClaim = await _userManager.GetClaimsAsync(user);
            if(existClaim != null)
                await _userManager.RemoveClaimsAsync(user, existClaim);

            var claimadd = request.Claims.Where(x => x.IsSelected).Select(c => new Claim(c.ClaimType, c.IsSelected.ToString())).ToList();

            await _userManager.AddClaimsAsync(user, claimadd);
            _notyf.Success("Update Claims Succeeded");
            return RedirectToAction(nameof(Index));
                

        }
     }
}
