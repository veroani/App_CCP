using App_CCP.Models;
using App_CCP.Services;
using App_CCP.Services.Interfaces;
using App_CCP.View_Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using App_CCP.Data;

namespace App_CCP.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly IAccountService accountService;
        private readonly ICustomEmailSender emailSender;
        private readonly AppDbContext _context;

        public AccountController(
            SignInManager<Users> signInManager,
            UserManager<Users> userManager,
            IAccountService accountService,
            ICustomEmailSender emailSender,
            AppDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.accountService = accountService;
            this.emailSender = emailSender;
            _context = context;
        }

        // Login - GET
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            if (!ModelState.IsValid)
                return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email sau parolă incorectă.");
                return View(model);
            }

            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                // Permite autentificarea pentru admin, chiar dacă emailul nu e confirmat
            }
            else if (!await userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("", "Contul nu a fost confirmat. Verifică adresa de email.");
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (user.IsFirstLogin)
                {
                    // Reîncarcă utilizatorul direct din baza de date pentru a avea toate proprietățile
                    var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                    if (dbUser != null)
                    {
                        dbUser.IsFirstLogin = false;
                        _context.Users.Update(dbUser);
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Bine ai venit! Te rugăm să completezi toate datele din profil pentru a te bucura de avantajele aplicației. " +
                        "<a href='/Home/Terms' class='text-decoration-underline ms-1'>Vezi condițiile generale</a>";

                    return RedirectToAction(nameof(ProfileController.Edit), "Profile");
                }

                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Contul este blocat temporar din cauza încercărilor eșuate.");
            }
            else
            {
                ModelState.AddModelError("", "Email sau parolă incorectă.");
            }

            return View(model);
        }

        // GET: /Account/Register
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new Users
            {
                FullName = model.Name,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = "Nespecificat",
                Address = "Nespecificat",
                PlaceOfBirth = "Nespecificat",
                DateOfBirth = null,
                Nationality = "Nespecificat",
                IsFirstLogin = true
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account",
                    new { userId = user.Id, token }, Request.Scheme);

                await emailSender.SendEmailAsync(user.Email, "Confirmare Cont",
                    $"Te rugăm să îți confirmi contul apăsând <a href='{confirmationLink}'>aici</a>.");

                TempData["SuccessMessage"] = "Înregistrare reușită! Verifică emailul pentru a confirma contul.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return RedirectToAction(nameof(HomeController.Index), "Home");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Email confirmat cu succes! Te poți autentifica acum.";
                return RedirectToAction(nameof(Login));
            }

            TempData["ErrorMessage"] = "Confirmarea emailului a eșuat.";
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }


        // Forgot Password - GET
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Forgot Password - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                var token = await userManager.GeneratePasswordResetTokenAsync(user);

                var callbackUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token, email = user.Email },
                    protocol: Request.Scheme);
                if (string.IsNullOrEmpty(user?.Email))
                {
                    // Ceva nu e în regulă, trimite eroare sau tratează
                    ModelState.AddModelError("", "Nu exista niciun e-mail asociat acestui cont.");
                    return View(model);
                }

                await emailSender.SendEmailAsync(user.Email, "Reset Password", 
                    $"Poți să-ți resetezi parola dând clic <a href='{callbackUrl}'>aici</a>.");

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        // ForgotPassword Confirmation
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // Reset Password - GET
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Token de resetare invalid.");
                return RedirectToAction("ForgotPassword");
            }

            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        // Reset Password - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // Reset Password Confirmation
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<Users>
        {
            public CustomClaimsPrincipalFactory(
                UserManager<Users> userManager,
                IOptions<IdentityOptions> optionsAccessor)
                : base(userManager, optionsAccessor)
            { }

            protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Users user)
            {
                var identity = await base.GenerateClaimsAsync(user);

                // Adauga claim-uri personalizate
                identity.AddClaim(new Claim("ProfilePicture", user.ProfilePicture ?? "/images/default-profile.png"));
                identity.AddClaim(new Claim("FullName", user.FullName ?? user.UserName ?? "Utilizator"));

                // Adauga explicit rolurile utilizatorului
                var roles = await UserManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }

                return identity;
            }
        }


    }
}