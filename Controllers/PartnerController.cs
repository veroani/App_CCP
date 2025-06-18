using App_CCP.Data;
using App_CCP.Models;
using App_CCP.View_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace App_CCP.Controllers
{
    public class PartnerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        // Constructorul controllerului - injectam AppDbContext pentru a accesa baza de date
        public PartnerController (AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region Listarea Partenerilor pentru utilizatori
        // Afiseaza lista partenerilor pentru utilizatori (toti vizitatorii aplicatiei)
        public IActionResult Index()
        {
            var partners = _context.Partners.ToList(); // Obtine lista tuturor partenerilor din DB
            return View(partners); // Trimite lista de parteneri la view
        }
        #endregion

        #region Gestionare Parteneri pentru Admin
        // Afiseaza lista partenerilor pentru Admin
        [Authorize(Roles = "Admin")] // Permite accesul doar utilizatorilor cu rolul de Admin
        public IActionResult Manage(string searchQuery)
        {
            var partners = _context.Partners.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                partners = partners.Where(p => p.Name.Contains(searchQuery));
            }

            return View(partners.ToList());
        }


        // Adauga un nou partener (formular de adaugare)

        [Authorize(Roles = "Admin")] // Permite accesul doar Admin-ilor
        [HttpGet] // Afiseaza formularul de adaugare
        public async Task<IActionResult> AddPartner()
        {
            var users = await _userManager.GetUsersInRoleAsync("Partner");

            ViewBag.UsersList = new SelectList(users, "Id", "FullName");

            return View(); // Afiseaza formularul gol pentru adaugare partener
        }

        // Adauga un partener in baza de date (procesare a formularului de adaugare)

        [HttpPost] // Procesare date dupace utilizatorul trimite formularul
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPartner(Partners partner, IFormFile LogoFile)
        {
            if (LogoFile != null && LogoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await LogoFile.CopyToAsync(stream);
                }

                partner.LogoUrl = "/images/" + fileName; // Cale relativă, bună pentru <img>
            }

            if (ModelState.IsValid) // Verifica daca datele sunt valide
            {
                _context.Partners.Add(partner); // Adauga partenerul in baza de date
                await _context.SaveChangesAsync();  // Salveaza odificarile
                return RedirectToAction("Manage");  // Redirectioneaza la pagina de gestionare
            }

            return View(partner); // Daca datele nu sunt valide, reafiseaza formularul cu erori
        }



        // Editare partener (formular de editare)

        [HttpGet]
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> EditPartner(int id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null)
                return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Partner") && partner.UserId != currentUserId)
                return Forbid();

            // Doar adminul primeste lista de utilizatori
            if (User.IsInRole("Admin"))
            {
                var users = await _userManager.GetUsersInRoleAsync("Partner");
                ViewBag.UsersList = new SelectList(users, "Id", "FullName", partner.UserId);
            }
            return View(partner);
        }

        // Modifica un partener existent

        [Authorize(Roles = "Admin,Partner")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPartner(Partners model, IFormFile? LogoFile)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var partner = await _context.Partners.FirstOrDefaultAsync(p => p.Id == model.Id);

            if (partner == null)
                return NotFound();

            if (User.IsInRole("Partner") && partner.UserId != currentUserId)
                return Forbid();

            // Procesare imagine (daca e trimisa)
            if (LogoFile != null && LogoFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(LogoFile.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("LogoFile", "Fișierul trebuie să fie o imagine validă.");
                }
                else
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid() + ext;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await LogoFile.CopyToAsync(stream);

                    partner.LogoUrl = "/images/" + fileName;
                }
            }

            // Curatare erori false de la LogoFile
            if (ModelState.TryGetValue("LogoFile", out var logoEntry) && logoEntry.Errors.Count > 0)
            {
                var onlyLogoError = ModelState.ErrorCount == logoEntry.Errors.Count;
                if (onlyLogoError)
                {
                    logoEntry.Errors.Clear();
                }
            }

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    var users = await _userManager.GetUsersInRoleAsync("Partner");
                    ViewBag.UsersList = new SelectList(users, "Id", "FullName", model.UserId);
                }

                return View(model);
            }

            // Actualizare câmpuri
            partner.Name = model.Name;
            partner.Description = model.Description;
            partner.DiscountDetails = model.DiscountDetails;
            partner.WebsiteUrl = model.WebsiteUrl;

            // Doar adminul poate schimba utilizatorul asociat
            if (User.IsInRole("Admin"))
            {
                partner.UserId = model.UserId;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Partenerul a fost actualizat cu succes.";

            return RedirectToAction("EditPartner", new { id = partner.Id });
        }

        // sterge un partener

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> DeletePartner(int id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null)
                return NotFound();

            return View(partner); // pagina DeletePartner.cshtml
        }

        [Authorize(Roles = "Admin")] // Permite accesul doar Admin-ilor
        [HttpPost, ActionName("DeletePartner")] // Atribut pentru a proteja actiunea de stergere
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var partner = _context.Partners.Find(id); // Cautam partenerul dupaID
            if (partner != null)
            {
                _context.Partners.Remove(partner); // sterge partenerul din baza de date
                _context.SaveChanges(); // Salveaza modificarile
            }
            return RedirectToAction("Manage"); // Redirectioneaza la pagina de gestionare
        }

        // cauta utilizatorii cu card activ
        
        [Authorize(Roles = "Partner,Admin")] // Doar partenerii si adminii pot accesa
        public async Task<IActionResult> ValidCardUsers(string searchQuery)
        {
            var today = DateTime.UtcNow.Date;
            var usersQuery = _context.Users
                .Where(u => u.ExpirationDate >= today) // Selectează doar utilizatorii cu card valabil
                .AsQueryable();

            // Dacă există un criteriu de căutare, aplică filtrul
            if (!string.IsNullOrEmpty(searchQuery))
            {
                usersQuery = usersQuery.Where(u =>
                    u.FullName.Contains(searchQuery) ||
                    u.UniqueCode.Contains(searchQuery));
            }

            var usersWithValidCards = await usersQuery
                .Select(u => new ValidCardUserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    UniqueCode = u.UniqueCode,
                    ExpirationDate = u.ExpirationDate
                })
                .ToListAsync();

            return View(usersWithValidCards);
        }

        [Authorize(Roles = "Partner")]
        [HttpGet]
        public async Task<IActionResult> EditOwnProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var partner = await _context.Partners.FirstOrDefaultAsync(p => p.UserId == userId);

            if (partner == null)
            {
                return View("NoPartnerAssigned"); 
            }

            return View(partner);
        }

        [Authorize(Roles = "Partner")]
        [HttpPost]
        public async Task<IActionResult> EditOwnProfile(Partners model, IFormFile? LogoFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var partner = await _context.Partners.FirstOrDefaultAsync(p => p.Id == model.Id && p.UserId == userId);

            if (partner == null)
                return Forbid(); // Securitate: să nu poată edita alt partener

            // Gestionare fișier logo
            if (LogoFile != null && LogoFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(LogoFile.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("LogoFile", "Fișierul trebuie să fie o imagine validă.");
                }
                else
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + ext;
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await LogoFile.CopyToAsync(stream);
                    }

                    partner.LogoUrl = "/images/" + fileName;
                }
            }

            // daca doar LogoFile a fost invalid, sterge eroarea
            if (ModelState.TryGetValue("LogoFile", out var logoEntry) && logoEntry.Errors.Count > 0)
            {
                var onlyLogoError = ModelState.ErrorCount == logoEntry.Errors.Count;
                if (onlyLogoError)
                {
                    logoEntry.Errors.Clear();
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Actualizare campuri
            partner.Name = model.Name;
            partner.Description = model.Description;
            partner.DiscountDetails = model.DiscountDetails;
            partner.WebsiteUrl = model.WebsiteUrl;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profilul a fost actualizat cu succes.";
            return RedirectToAction("EditOwnProfile");
        }

        #endregion
    }
}