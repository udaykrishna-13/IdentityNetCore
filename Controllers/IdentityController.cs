using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityNetCore.Models;
using IdentityNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityNetCore.Controllers
{
    public class IdentityController : Controller
    {
       private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SignInManager<IdentityUser> _signInManager { get; }
        public IEmailSender EmailSender { get; }

        public IdentityController(UserManager<IdentityUser> userManager,SignInManager<IdentityUser> signInManager,RoleManager<IdentityRole> roleManager,IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this._roleManager = roleManager;
            EmailSender = emailSender;
        }
        [Authorize]
        public async Task<IActionResult> MFASetup()
        {
            string provider = "AspIdentity";
            var user = await _userManager.GetUserAsync(User);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            var token = await _userManager.GetAuthenticatorKeyAsync(user);
            var qrCodeUrl = $"otpauth://topt/{provider}:{user.Email}?secret={token}&issuer={provider}&digits=6";
            var model = new MFASetupViewModel() { Token=token,QrCodeUrl=qrCodeUrl};
            return View(model);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MFASetup(MFASetupViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                var user = await _userManager.GetUserAsync(User);
                var succeed = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code);
                if (succeed)
                {
                    await _userManager.SetTwoFactorEnabledAsync(user, true);
                    return RedirectToAction("index", "Home");
                }
                else
                {
                    ModelState.AddModelError("MFA setup", "MFA could not be validated");
                }
            }
            return View(model);
        }

        public IActionResult SignUp()
        {
            return View(new SignUpViewModel() { 
            Role="Member"
            });
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (ModelState.IsValid)
            {

                if(!await _roleManager.RoleExistsAsync(model.Role))
                {
                    var role = new IdentityRole() { Name = model.Role };
                    var roleResult= await _roleManager.CreateAsync(role);

                    if (!roleResult.Succeeded)
                    {
                        var errors = roleResult.Errors.Select(x => x.Description);
                        ModelState.AddModelError("Role", string.Join(",", errors));
                        return View(model);
                    }
                }

                if((await _userManager.FindByEmailAsync(model.Email)) == null)
                {
                    var user = new IdentityUser {
                        Email = model.Email,
                        UserName=model.Email
                    };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    
                    if (result.Succeeded)
                    {
                        var claim = new Claim("Department", model.Department);
                        await _userManager.AddClaimAsync(user, claim);
                        await _userManager.AddToRoleAsync(user, model.Role);//add role to user
                        user = await _userManager.FindByNameAsync(model.Email);

                        /***   Code for email confirmation
                        
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var link = Url.ActionLink("ConfirmEmail", "Identity", new { userid=user.Id, token=token });
                        await EmailSender.SendEmailAsync("udaykrishna13@outlook.Com", user.Email,  "Confirm you email", $"Hello,<br> please verify email by clicking link <br> {link}");

                        *****/
                        return RedirectToAction("SignIn");
                    }

                    ModelState.AddModelError("SignUp", string.Join("", result.Errors.Select(x => x.Description)));
                }
            }
            return View(model);
        }

        public async Task<IActionResult>ConfirmEmail(string UserId,string token)
        {
            var user = await _userManager.FindByIdAsync(UserId);

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return RedirectToAction("SignIn");
            }

            return new NotFoundResult();
        }
        public IActionResult SignIn()
        {
            return View(new SignInViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
          
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.RequiresTwoFactor) 
                {
                    return RedirectToAction("MFACheck");
                }
                else
                {
                    if (result.Succeeded)
                    {
                       return RedirectToAction("Index", "Home",null);
                    }
                    else
                    {
                        ModelState.AddModelError("Error", "login failed, please try again");
                    }
                }
                
            }
            
            return View(model);
            
        }
        public IActionResult MFACheck()
        {
            return View(new MFACheckViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> MFACheck(MFACheckViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, false, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home",null);
                }
            }
            return View(model);
        }
        public async Task<IActionResult> AccessDenied()
        {
            return View("AccessDenied");
        }
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("SignIn", "Identity");
        }
    }
}
