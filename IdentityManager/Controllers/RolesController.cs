using AspNetCoreHero.ToastNotification.Abstractions;
using IdentityManager.Data;
using IdentityManager.Dtos;
using IdentityManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityManager.Controllers
{
    [Authorize(Roles ="Admin")]
    public class RolesController : Controller
    {

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly INotyfService _notyf;

        public RolesController(RoleManager<IdentityRole> roleManager, AppDbContext context, INotyfService notyf)
        {
            _roleManager = roleManager;
            _context = context;
            _notyf = notyf;
        }
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles.ToListAsync();
            var listroles = roles.Select(x => new RoleDto
            {
                Name = x.Name,
                Id = x.Id
            }).ToList();
            return View(listroles);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(RoleDto request)
        {
            if(ModelState.IsValid)
            {
               var rs = await _roleManager.CreateAsync(new IdentityRole(request.Name));
                if (rs.Succeeded)
                {
                    _notyf.Success("Created Succecced");
                    return RedirectToAction(nameof(Index));
                }

            }
            _notyf.Warning("Created failed");
            return View(request);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var role =await _roleManager.FindByIdAsync(id);
            return View(new RoleDto { Id=role.Id,Name=role.Name});
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RoleDto request)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(request.Id);
                role.Name = request.Name;
                var rs = await _roleManager.UpdateAsync(role);
                if (rs.Succeeded)
                {
                    _notyf.Success("Edit Succecced");
                    return RedirectToAction(nameof(Index));
                }
            }
            _notyf.Warning("Edit failed");
            return View(request);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            await _roleManager.DeleteAsync(role);
            _notyf.Success("Delete Succecced");
            return RedirectToAction(nameof(Index));
        }
    }
}
