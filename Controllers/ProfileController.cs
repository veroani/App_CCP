using App_CCP.Data;
using App_CCP.Models;
using App_CCP.View_Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;


namespace App_CCP.Controllers
{
   [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly SignInManager<Users> _signInManager;

        // Constructorul pentru ProfileController
        public ProfileController(UserManager<Users> userManager, AppDbContext dbContext, IWebHostEnvironment webHostEnvironment, SignInManager<Users> signInManager)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _signInManager = signInManager;
        }
        
        private IEnumerable<SelectListItem> GetOccupationList()
        {
            return Enum.GetValues(typeof(Occupation))
                .Cast<Occupation>()
                .Select(o => new SelectListItem
                {
                    Value = o.ToString(),
                    Text = o.ToString()
                });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account"); // Redirecționăm utilizatorul la pagina de login dacă nu este autentificat
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                Address = user.Address,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Nationality = user.Nationality,
                DateOfBirth = user.DateOfBirth,
                Age = user.Age ?? 0,
                PlaceOfBirth = user.PlaceOfBirth,
                Occupation = user.Occupation,
                Mentions = user.Mentions ?? string.Empty,
                ProfilePicturePath = string.IsNullOrEmpty(user.ProfilePicture) ? null : user.ProfilePicture, // Setează calea imaginii dacă există// calea imaginii existente
                OccupationList = GetOccupationList(),
                // Generarea linkului pentru card
                ProfileCardUrl = Url.Action("GenerateCard", "Card", new { userId = user.Id })
            };

            return View(model);
        }
        // Metoda GET pentru editarea profilului
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                Address = user.Address,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Nationality = user.Nationality,
                DateOfBirth = user.DateOfBirth,
                PlaceOfBirth = user.PlaceOfBirth,
                Occupation = user.Occupation,
                Mentions = user.Mentions ?? string.Empty,
                ProfilePicturePath = user.ProfilePicture,
                OccupationList = GetOccupationList(),
                ProfileCardUrl = Url.Action("GenerateCard", "Card", new { userId = user.Id }),
                ExpirationDate = user.ExpirationDate,
                UniqueCode = user.UniqueCode,
                Age = user.Age ?? 0
            };
            return View(model);
        }
        // Metoda POST pentru actualizarea profilului
        [HttpPost]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.FullName;
                user.Address = model.Address;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Nationality = model.Nationality;
                user.DateOfBirth = model.DateOfBirth;
                user.PlaceOfBirth = model.PlaceOfBirth;
                user.Occupation = model.Occupation;
                user.Mentions = model.Mentions ?? string.Empty;
                user.ExpirationDate = model.ExpirationDate ?? DateTime.Now.AddYears(1);
                user.UniqueCode = model.UniqueCode;

                if (model.ProfilePicture != null)
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profile_photo");

                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ProfilePicture.FileName)}";
                    var fullPath = Path.Combine(filePath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }

                    user.ProfilePicture = "/images/profile_photo/" + fileName;
                }

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Obtine rolurile curente
                    var roles = await _userManager.GetRolesAsync(user);

                    // Construieste noile claims
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? "Utilizator"),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("FullName", user.FullName ?? user.UserName ?? "Utilizator"),
                new Claim("ProfilePicture", user.ProfilePicture ?? "/images/default-profile.png")
            };

                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    // Reconstruieste principalul
                    var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // inlocuieșse identitatea activa
                    await _signInManager.SignOutAsync();
                    await _signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, principal);

                    TempData["SuccessMessage"] = "Profilul a fost actualizat cu succes!";
                    return RedirectToAction("Index");
                }


                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error: {error.Description}");
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
                
            return View(model);
        }
    }
}