using App_CCP.View_Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace App_CCP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }

        public IActionResult Privacy()
        {
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            string privacyPolicyPath = Path.Combine(uploadPath, "PrivacyPolicy.pdf");

            if (System.IO.File.Exists(privacyPolicyPath))
            {
                ViewBag.PrivacyPolicyExists = true;
            }
            else
            {
                ViewBag.PrivacyPolicyExists = false;
            }

            return View();
        }

        public IActionResult Terms()
        {
            string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            string termsPath = Path.Combine(uploadPath, "TermsConditions.pdf");

            if (System.IO.File.Exists(termsPath))
            {
                ViewBag.TermsExists = true;
            }
            else
            {
                ViewBag.TermsExists = false;
            }

            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
     
    }
}
