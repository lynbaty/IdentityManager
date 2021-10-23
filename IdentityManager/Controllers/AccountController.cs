using IdentityManager.Dtos;
using IdentityManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityManager.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, 
            RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }
            var list = _roleManager.Roles.Select(x => 
                new SelectListItem
                {
                    Value = x.Name,
                    Text = x.Name
                }
            ).ToList();
            var registerDto = new RegisterDto() { RoleList=list};
            return View(registerDto);
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnurl=null)
        {
            var redirecturl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnurl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirecturl);
            return Challenge(properties, provider);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string remoteError=null, string returnurl = null)
        {
            returnurl = returnurl ?? Url.Content("~/");
            if (remoteError!=null)
            {
                ModelState.AddModelError("", $"Error from external provider:{remoteError}");
            }
            var info =await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToAction(nameof(Login));
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,isPersistent: false);
            if(result.Succeeded)
            {
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                return LocalRedirect(returnurl);
            }
            else
            {
                ViewData["ReturnUrl"] = returnurl;
                ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                return View("ExternalLoginConfirmed", new ExternalLoginDto { Email = email,Name = name });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginConfirmed(ExternalLoginDto request, string returnurl = null)
        {
            returnurl = returnurl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null) return View("Error");
                var user = new ApplicationUser { Email = request.Email, Name = request.Name, UserName = request.Email };
                var result = await _userManager.CreateAsync(user);
                if(result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    var rs = await _userManager.AddLoginAsync(user, info);
                    if (rs.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                        return LocalRedirect(returnurl);
                    }
                }
                AddError(result);
            }
            return View(request);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto request)
        {
           
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    Name = request.Name,
                    UserName = request.Email,
                    Email = request.Email,
                };
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    if(!string.IsNullOrEmpty(request.RoleSelected))
                    {
                        await _userManager.AddToRoleAsync(user, request.RoleSelected);
                    }    
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { code = code, userId = user.Id },HttpContext.Request.Scheme);

                    await _emailSender.SendEmailAsync(request.Email, "Confirm Account", "Please click <a href=\"" + callbackUrl + "\">here</a> to confirmed your account");
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                AddError(result);
            }
            return View(request);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code = null)
        {
            if (code == null) return View("Error");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return View("Error");
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded) return View();
            return View("Error");

        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnurl = null)
        {
            ViewData["ReturnUrl"] = returnurl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto request, string returnurl = null)
        {
            ViewData["returnurl"] = returnurl;
            returnurl = returnurl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, request.Remember, true);
                if (result.IsLockedOut) return View("Lockout");
                if (result.Succeeded) return LocalRedirect(returnurl);
                ModelState.AddModelError("", "User or password not correct");
            }
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmed()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmed()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if(user==null) return View("ForgotPasswordConfirmed");
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword","Account",new{userId = user.Id,code = code},HttpContext.Request.Scheme);

                await _emailSender.SendEmailAsync(request.Email,"Password Reset","Please <a href=\""+callbackUrl+"\">click here</a> to reset your password");
            }
            return RedirectToAction("ForgotPasswordConfirmed","Account");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string userId, string code=null)
        {
            if(code==null) return View("error");
            var request = new ResetPasswordDto{Code=code,UserId=userId};
            return View(request);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
        {
            if (ModelState.IsValid)
            {
              var user =await _userManager.FindByIdAsync(request.UserId);
              if(user==null) return RedirectToAction("ResetPasswordConfirmed");
              var result =await _userManager.ResetPasswordAsync(user,request.Code,request.NewPassword);
              if(result.Succeeded) 
              {
                  return RedirectToAction("ResetPasswordConfirmed","Account");
              }
              AddError(result);
            }
            
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            var token = await _userManager.GetAuthenticatorKeyAsync(user);
            var model = new TwoFactorAuthenticator { Token = token };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EnableAuthenticator(TwoFactorAuthenticator request)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var succeeded = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code);
                if (succeeded) await _userManager.SetTwoFactorEnabledAsync(user, true);
                ModelState.AddModelError("Verify", "Your two factor auth code could not be avalidated");
                return View(request);
            }
            return RedirectToAction("AuthenticatorComfirmed");
        }

        private void AddError(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}