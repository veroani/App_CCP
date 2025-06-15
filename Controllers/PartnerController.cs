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

        [Authorize(Roles = "Admin")] 
        [HttpPost] // Procesare date dupace utilizatorul trimite formularul
        [ValidateAntiForgeryToken]
        public IActionResult AddPartner(Partners partner)
        {
            if (ModelState.IsValid) // Verifica daca datele sunt valide
            {
                _context.Partners.Add(partner); // Adauga partenerul in baza de date
                _context.SaveChanges(); // Salveaza odificarile
                return RedirectToAction("Manage"); // Redirectioneaza la pagina de gestionare
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
        [HttpPost] // Procesare date dupa ce utilizatorul trimite formularul
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPartner(Partners partner)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingPartner = await _context.Partners.AsNoTracking().FirstOrDefaultAsync(p => p.Id == partner.Id);
            if (existingPartner == null)
                return NotFound();

            // Restricție pentru partener: nu poate modifica UserId
            if (User.IsInRole("Partner") && existingPartner.UserId != currentUserId)
                return Forbid();

            if (ModelState.IsValid)
            {
                // Dacă nu e admin, păstrează UserId-ul original
                if (!User.IsInRole("Admin"))
                {
                    partner.UserId = existingPartner.UserId;
                }

                _context.Partners.Update(partner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Partenerul a fost actualizat cu succes.";
                return RedirectToAction("EditPartner", new { id = partner.Id });
            }

            // Reîncarcă lista de utilizatori în caz de eroare de validare
            if (User.IsInRole("Admin"))
            {
                var users = await _userManager.GetUsersInRoleAsync("Partner");
                ViewBag.UsersList = new SelectList(users, "Id", "FullName", partner.UserId);
            }
            return View(partner);
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
        public async Task<IActionResult> EditOwnProfile(Partners model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var partner = await _context.Partners.FirstOrDefaultAsync(p => p.Id == model.Id && p.UserId == userId);

            if (partner == null)
                return Forbid(); // Securitate: să nu poată edita alt partener

            if (ModelState.IsValid)
            {
                partner.Name = model.Name;
                partner.Description = model.Description;
                partner.DiscountDetails = model.DiscountDetails;
                partner.LogoUrl = model.LogoUrl;
                partner.WebsiteUrl = model.WebsiteUrl;

                _context.Partners.Update(partner);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profilul a fost actualizat cu succes.";
                return RedirectToAction("EditOwnProfile");
            }

            return View(model);
        }

        #endregion
    }
}