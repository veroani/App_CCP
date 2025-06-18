using App_CCP.Data;
using App_CCP.View_Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace App_CCP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment, AppDbContext context)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _context = context;
        }

        public IActionResult Privacy()
        {
            var doc = _context.Documents
                .Where(d => d.Type == "PrivacyPolicy" && d.IsActive)
                .OrderByDescending(d => d.UploadDate)
                .FirstOrDefault();

            ViewBag.Document = doc;
            return View();
        }

        public IActionResult Terms()
        {
            var doc = _context.Documents
                .Where(d => d.Type == "TermsConditions" && d.IsActive)
                .OrderByDescending(d => d.UploadDate)
                .FirstOrDefault();

            ViewBag.Document = doc;
            return View();
        }

        [HttpGet("/documente/descarca/{id}")]
        public IActionResult Download(int id)
        {
            var doc = _context.Documents.FirstOrDefault(d => d.Id == id && d.IsActive);
            if (doc == null) return NotFound();

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", doc.FileName);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return File(stream, doc.ContentType, doc.OriginalName);
        }

        public IActionResult Index()
        {
            var partners = _context.Partners.ToList(); 
            return View(partners);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
     
    }
}
