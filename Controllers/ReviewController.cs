using App_CCP.Data;
using App_CCP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace App_CCP.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        // Afișează review-urile (pentru utilizatori)
        public IActionResult Index()
        {
            var reviews = _context.Reviews
                .Where(r => r.IsPublished)
                .OrderByDescending(r => r.DateCreated)
                .ToList();

            return View(reviews);
        }

        // Permite utilizatorilor să adauge review-uri
        [HttpGet]
        public IActionResult AddReview()
        {
            return View();
        }

        [HttpPost]
        [HttpPost]
        public IActionResult AddReview(Review review)
        {
            if (ModelState.IsValid)
            {
                review.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                review.UserName = string.IsNullOrWhiteSpace(review.Alias)
    ? "Anonim"
    : review.Alias;

                review.IsPublished = false;
                review.DateCreated = DateTime.Now;

                _context.Reviews.Add(review);
                _context.SaveChanges();
                return RedirectToAction("Index"); // dupa adaugarea review-ului, se redirectioneaza la lista de review-uri
            }

            return View(review); // Daca există erori de validare, returnează formularul cu erorile
        }

        // afiseaza review-urile nepublicate pentru admini
        [Authorize(Roles = "Admin")]
        public IActionResult Manage()
        {
            var reviews = _context.Reviews.ToList();
            return View(reviews);
        }

        // publica review-ul
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult PublishReview(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review != null)
            {
                review.IsPublished = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Manage");
        }

        [AllowAnonymous]
        public IActionResult PublishedReviews()
        {
            var publishedReviews = _context.Reviews
                .Where(r => r.IsPublished)
                .OrderByDescending(r => r.DateCreated) // le vedem pe cele mai recente primele
                .ToList();

            return View("PublishedReviews", publishedReviews);
        }

        // sterge un review
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult DeleteReview(int id)
        {
            var review = _context.Reviews.Find(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                _context.SaveChanges();
            }
            return RedirectToAction("Manage");
        }
    }
}