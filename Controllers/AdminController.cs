using App_CCP.Models;
using App_CCP.Services.Interfaces;
using App_CCP.View_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace App_CCP.Controllers
{
    [Authorize(Roles = "Admin")]// Doar utilizatorii cu rolul "Admin" au acces
    public class AdminController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICustomEmailSender _emailSender;


        public AdminController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment webHostEnvironment, ICustomEmailSender emailSender)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _emailSender = emailSender;
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

        public IActionResult Statistics(DateTime? expirationDate, string uniqueCode)
        {
            // Filtrăm utilizatorii pe baza datei de expirare a cardului
            var query = _userManager.Users.AsQueryable();

            // Filtrare după data de expirare
            if (expirationDate.HasValue)
            {
                query = query.Where(u => u.ExpirationDate.Date <= expirationDate.Value.Date);
            }

            // Filtrare după codul unic
            if (!string.IsNullOrEmpty(uniqueCode))
            {
                query = query.Where(u => u.UniqueCode.Contains(uniqueCode)); // Poți schimba cu `==` pentru un meci exact
            }

            // Obține utilizatorii care se potrivesc criteriilor
            var users = query.ToList();

            // Numărul de utilizatori care au rezultat din căutare
            var totalCount = users.Count();

            // Trimitem utilizatorii și numărul acestora în View
            ViewBag.TotalCount = totalCount;

            return View(users);
        }


        // Lista utilizatorilor

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); //important

                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    IsAdmin = roles.Contains("Admin"),
                    IsPartner = roles.Contains("Partner"),
                    Roles = roles.ToList() // umplem lista
                });
            }

            return View(model);
        }
        public async Task<IActionResult> ManageRoles()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    IsAdmin = roles.Contains("Admin"),
                    IsPartner = roles.Contains("Partner"),
                    Roles = roles.ToList() //aici era lipsa
                });
            }

            return View(model);
        }
 
        [HttpPost]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                return RedirectToAction("ManageRoles");
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                TempData["SuccessMessage"] = "Rolul de Admin a fost revocat.";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                TempData["SuccessMessage"] = "Rolul de Admin a fost acordat.";
            }

            return RedirectToAction("ManageRoles");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePartnerRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                return RedirectToAction("ManageRoles");
            }

            if (await _userManager.IsInRoleAsync(user, "Partner"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Partner");
                TempData["SuccessMessage"] = "Rolul de Partener a fost revocat.";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Partner");
                TempData["SuccessMessage"] = "Rolul de Partener a fost acordat.";
            }

            return RedirectToAction("ManageRoles");
        }


        public async Task<IActionResult> UserList(string searchQuery)
        {
            // obtinem toti utilizatorii
            var usersQuery = _userManager.Users.AsQueryable();

            // daca exista un termen de cautat, aplicam filtrul
            if (!string.IsNullOrEmpty(searchQuery))
            {
                usersQuery = usersQuery.Where(u =>
                    (u.UserName ?? string.Empty).Contains(searchQuery) ||
                    (u.Email ?? string.Empty).Contains(searchQuery) ||
                    (u.FullName ?? string.Empty).Contains(searchQuery) ||  // Căutare după nume complet
                    (u.UniqueCode ?? string.Empty).Contains(searchQuery));
            }

            // se executa interogarea si se obtine lista de utilizatori
            var users = await usersQuery.ToListAsync();
            ViewBag.UserManager = _userManager;

            // intarziere artificiala pentru a simula o cautare de durata
            await Task.Delay(1000);

            return View(users);
        }

        // Afiseaza formularul de editare pentru un utilizator
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();


            var model = new AdminEditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty, // Daca este null, se seteaza un string gol
                UserName = user.UserName ?? string.Empty,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Nationality = user.Nationality,
                DateOfBirth = user.DateOfBirth,
                Age = user.Age ?? 0,
                PlaceOfBirth = user.PlaceOfBirth,
                Occupation = user.Occupation,
                ProfilePicturePath = string.IsNullOrEmpty(user.ProfilePicture) ? null : user.ProfilePicture,
                OccupationList = GetOccupationList(),
                ExpirationDate = user.ExpirationDate,
                UniqueCode = user.UniqueCode,
                ConcurrencyStamp = user.ConcurrencyStamp

            };
            if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("Email", "Adresa de email este invalidă.");
                return View(model);
            }
            return View(model);
        }

        public async Task<IActionResult> PendingCards()
        {
            var pendingUsers = await _userManager.Users
                .Where(u => (!u.IsCardApprovedByAdmin || u.IsCardRevoked))
                .ToListAsync();

            return View(pendingUsers);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCard(string userId, string? returnTo = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsCardApprovedByAdmin = true;
            user.IsCardRevoked = false;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"Cardul pentru {user.FullName} a fost aprobat.";

            if (!string.IsNullOrEmpty(returnTo) && returnTo == "PendingCards")
                return RedirectToAction("PendingCards");

            return RedirectToAction("ViewUser", new { id = userId });
        }

        public async Task<IActionResult> RevokeCard(string userId, string? returnTo = null)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsCardRevoked = true;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"Cardul pentru {user.FullName} a fost revocat.";

            if (!string.IsNullOrEmpty(returnTo) && returnTo == "PendingCards")
                return RedirectToAction("PendingCards");

            return RedirectToAction("ViewUser", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> ViewUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Nationality = user.Nationality,
                DateOfBirth = user.DateOfBirth,
                Age = user.Age ?? 0,
                PlaceOfBirth = user.PlaceOfBirth,
                Occupation = user.Occupation,
                Mentions = user.Mentions,
                ProfilePicturePath = user.ProfilePicture,
                ExpirationDate = user.ExpirationDate,
                UniqueCode = user.UniqueCode,
            };

            return View("ViewUser", model);
        }


        // Actualizeaza datele utilizatorului
        [HttpPost]
        public async Task<IActionResult> EditUser(AdminEditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return Json(new { success = false, message = "Utilizatorul nu a fost găsit." });
            }

            // Validare concurență
            if (user.ConcurrencyStamp != model.ConcurrencyStamp)
            {
                return Json(new
                {
                    success = false,
                    errors = new List<string> { "Datele au fost modificate de altcineva. Vă rugăm să reîncărcați pagina." }
                });
            }
            if (user == null)
            {
                return Json(new { success = false, message = "Utilizatorul nu a fost găsit." });
            }

            user.FullName = model.FullName ?? string.Empty;
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;
            user.Nationality = model.Nationality;
            user.DateOfBirth = model.DateOfBirth;
            user.PlaceOfBirth = model.PlaceOfBirth;
            user.Occupation = model.Occupation;
            model.OccupationList = GetOccupationList();
            user.ExpirationDate = model.ExpirationDate;
            user.UniqueCode = model.UniqueCode;
            user.ConcurrencyStamp = model.ConcurrencyStamp;

            if (model.ProfilePicture != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ProfilePicture", "Formatul fișierului nu este acceptat. Folosiți .jpg, .jpeg, .png sau .gif.");
                    return View(model);
                }

                if (model.ProfilePicture.Length > 2 * 1024 * 1024) // Max 2 MB
                {
                    ModelState.AddModelError("ProfilePicture", "Dimensiunea fișierului nu trebuie să depășească 2MB.");
                    return View(model);
                }

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
                return Json(new { success = true });
            }
            return Json(new
            {
                success = false,
                errors = result.Errors.Select(e => e.Description).ToList()
            });
        }

        // Sterge un utilizator
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return RedirectToAction(nameof(UserList));
        }
        [HttpPost]
        public async Task<IActionResult> SendPasswordResetLink(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                return RedirectToAction("ViewUser", new { id = userId });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                TempData["ErrorMessage"] = "Utilizatorul nu are un email valid.";
                return RedirectToAction("ViewUser", new { id = user.Id });
            }
            await _emailSender.SendEmailAsync( user.Email, "Resetare parolă inițiată de administrator",
                $"Administratorul a inițiat o resetare a parolei pentru contul tău. Click aici pentru a continua: <a href='{resetLink}'>Resetare parolă</a>");

            TempData["SuccessMessage"] = "Emailul de resetare a fost trimis cu succes.";
            return RedirectToAction("ViewUser", new { id = userId });
        }
        [HttpGet]
        public IActionResult UploadDocuments()
        {
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            var documents = Directory.Exists(uploadPath)
                ? Directory.GetFiles(uploadPath)
                    .Select(Path.GetFileName)
                    .Where(name => name != null)
                    .Select(name => name!)
                    .ToList()
                : new List<string>();

            ViewBag.Documents = documents;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocuments(IFormFile privacyPolicy, IFormFile termsConditions)
        {
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            bool uploaded = false;
            var errors = new List<string>();

            if (privacyPolicy != null)
            {
                if (privacyPolicy.ContentType == "application/pdf")
                {
                    string privacyPath = Path.Combine(uploadPath, "PrivacyPolicy.pdf");
                    using var stream = new FileStream(privacyPath, FileMode.Create);
                    await privacyPolicy.CopyToAsync(stream);
                    uploaded = true;
                }
                else
                {
                    errors.Add("Fișierul Privacy Policy trebuie să fie în format PDF.");
                }
            }

            if (termsConditions != null)
            {
                if (termsConditions.ContentType == "application/pdf")
                {
                    string termsPath = Path.Combine(uploadPath, "TermsConditions.pdf");
                    using var stream = new FileStream(termsPath, FileMode.Create);
                    await termsConditions.CopyToAsync(stream);
                    uploaded = true;
                }
                else
                {
                    errors.Add("Fișierul Terms & Conditions trebuie să fie în format PDF.");
                }
            }

            if (uploaded)
            {
                TempData["Message"] = "Documentele au fost încărcate cu succes!";
            }
            else
            {
                errors.Add("Nu a fost selectat niciun fișier valid pentru încărcare.");
                TempData["Errors"] = errors;
            }

            return RedirectToAction("UploadDocuments");
        }

        [HttpPost]
        public IActionResult DeleteDocument(string documentName)
        {
            if (string.IsNullOrWhiteSpace(documentName))
            {
                TempData["Errors"] = new List<string> { "Numele documentului este invalid." };
                return RedirectToAction("UploadDocuments");
            }

            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            string filePath = Path.Combine(uploadPath, documentName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                TempData["Message"] = $"Documentul {documentName} a fost șters cu succes!";
            }
            else
            {
                TempData["Errors"] = new List<string> { $"Documentul {documentName} nu a fost găsit." };
            }

            return RedirectToAction("UploadDocuments");
        }
    }
}