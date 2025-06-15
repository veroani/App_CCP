using App_CCP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App_CCP.Models;
using App_CCP.View_Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace App_CCP.Controllers
{
    public class NewsController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        public NewsController(AppDbContext dbContext, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        //Pagina publica cu slider
        [AllowAnonymous]
        public async Task<IActionResult> Carousel()
        {
            var news = await _dbContext.NewsItems
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(news);
        }

        // Listare noutati pentru admini/parteneri
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> Index()
        {
            var news = await _dbContext.NewsItems
                .Include(x => x.Owner)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(news);
        }

        // Adaugare noutate
        [Authorize(Roles = "Admin,Partner")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> Create(NewsItemViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string? imagePath = null;

            if (model.ImageFile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Formatul imaginii nu este suportat.");
                    return View(model);
                }

                var folder = Path.Combine(_env.WebRootPath, "uploads", "news");
                Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid() + extension;
                var path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                imagePath = $"/uploads/news/{fileName}";
            }

            var user = await _dbContext.Users
                .Where(n => User.Identity != null && n.Email  != null && n.Email.Equals(User.Identity.Name))
                .FirstAsync();

            var news = new NewsItem
            {
                Title = model.Title ?? "Titlu implicit",
                Description = model.Description,
                ImagePath = imagePath,
                IsActive = model.IsActive,
                Owner = user
            };

            _dbContext.NewsItems.Add(news);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Noutatea a fost adăugată.";
            return RedirectToAction("Index");
        }

        // Editare
        [HttpGet]
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _dbContext.NewsItems.FindAsync(id);
            if (news == null) return NotFound();

            return View(new NewsItemViewModel
            {
                Id = news.Id,
                Title = news.Title,
                Description = news.Description,
                IsActive = news.IsActive,
                ExistingImagePath = news.ImagePath
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> Edit(NewsItemViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var news = await _dbContext.NewsItems.FindAsync(model.Id);
            if (news == null) return NotFound();

            news.Title = model.Title ?? "Titlu implicit";
            news.Description = model.Description;
            news.IsActive = model.IsActive;

            if (model.ImageFile != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Formatul imaginii nu este suportat.");
                    return View(model);
                }

                // Ștergere imagine veche
                if (!string.IsNullOrEmpty(news.ImagePath))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, news.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                // Salvare imagine nouă
                var folder = Path.Combine(_env.WebRootPath, "uploads", "news");
                Directory.CreateDirectory(folder);
                var fileName = Guid.NewGuid() + extension;
                var path = Path.Combine(folder, fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                news.ImagePath = $"/uploads/news/{fileName}";
            }
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Noutatea a fost actualizată.";
            return RedirectToAction("Index");
        }

        // Dezactivare 
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var news = await _dbContext.NewsItems.FindAsync(id);
            if (news == null) return NotFound();

            news.IsActive = !news.IsActive;
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _dbContext.NewsItems.FindAsync(id);
            if (news == null) return NotFound();

            if (!string.IsNullOrEmpty(news.ImagePath))
            {
                var imagePath = Path.Combine(_env.WebRootPath, news.ImagePath.TrimStart('/'));
                try
                {
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }
                catch (Exception)
                {
                   TempData["WarningMessage"] = "Imaginea nu a putut fi ștearsă, dar noutatea a fost eliminată.";
                }
            }

            _dbContext.NewsItems.Remove(news);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Noutatea a fost ștearsă.";
            return RedirectToAction("Index");
        }

    }
}