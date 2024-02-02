using IdentityApp.Data;
using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace IdentityApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        //private readonly IEmailSender _emailSender;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (model != null)
                {
                    await _signInManager.SignOutAsync();
                    /*if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("", "Hesabınızı onaylayınız.");
                    }*/
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user, null);

                        return RedirectToAction("Index", "Home");
                    }
                    else if (result.IsLockedOut)
                    {
                        var lockOutDate = await _userManager.GetLockoutEndDateAsync(user);
                        var timeLeft = lockOutDate.Value - DateTime.UtcNow;
                        ModelState.AddModelError("", $"Hesabınız kitlendi. {timeLeft} süre sonra tekrar deneyin.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Hatalı password.");
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Bu email bulunamadı.");
                }
            }
            return View(model);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                /*
                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var url = Url.Action("ConfirmEmail", "Account", new { user.Id, token });

                    _emailSender.SendEmailAsync(user.Email, "Hesap onayı", $"Lütfen Email hesabınızı onaylamak için linke <a>tıklayınız</a>.");

                    TempData["meesage"] = "Email hesabınızdaki onay mailine tıklayın.";
                    return RedirectToAction("Login", "Account");
                }*/
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            if (id == null && token == null)
            {
                TempData["meesage"] = "Geçersiz token bilgisi";
                return View();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    TempData["meesage"] = "Hesabınız onaylandı";
                    return RedirectToAction("Login", "Account");
                }
            }
            TempData["meesage"] = "Kullanıcı bulunamadı";
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
