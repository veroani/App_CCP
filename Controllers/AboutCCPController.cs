using App_CCP.View_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App_CCP.Controllers
{
    [Authorize]
    public class AboutCCPController : Controller
    {

    //Simulare: Stocare temporară a modelului in memorie
    private static AboutCCPViewModel _model = new AboutCCPViewModel
    {
        Titlu = "Cardul Cultura Plus",
        Descriere = "Cardul CCP oferă avantaje unice pentru clienții noștri...",
        Avantaje = new List<string> { "Acces la biblioteca Multimedia", "Acces gratuit sau reduceri la evenimente si ateliere", "..." },
        ImagineUrl = "../images/card-background.png"
    };

        [AllowAnonymous] // Permite accesul oricarui utilizator
        public IActionResult Index()
        {
            return View(_model);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit()
        {
            return View(_model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(AboutCCPViewModel model)
        {
            if (ModelState.IsValid)
            {
                _model = model;
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}