using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using App_CCP.Models; 
using App_CCP.View_Models;
using ZXing;
using Microsoft.AspNetCore.Authorization;

public class CardController : Controller
{
    private readonly UserManager<Users> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // Constructor
    public CardController(UserManager<Users> userManager, IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    
    [Authorize]
    public async Task<IActionResult> GenerateCard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Utilizatorul nu a fost găsit.");

        // Verifica daca utilizatorul este admin
        var isAdmin = User.IsInRole("Admin");

        // Daca nu este admin si cardul nu este aprobat, redirectioneaza
        if (!isAdmin && (!user.IsCardApprovedByAdmin || user.IsCardRevoked))
        {
            TempData["Warning"] = "Cardul tău nu este disponibil (neaprobat sau revocat).";
            return RedirectToAction("CardPending");
        }

        // Initializeaza codul si data expirarii doar daca lipsesc
        user.InitializeCardData();

        // Salveaza doar daca s-au modificat valorile
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return BadRequest("Nu s-au putut salva datele utilizatorului.");

        // Genereaza codul de bare SVG
        var barcodeSvg = GenerateBarcodeSvg(user.UniqueCode);

        // Creeaza modelul pentru afisare
        var model = new CardViewModel
        {
            FullName = user.FullName,
            ProfilePicture = user.ProfilePicture,
            ExpirationDate = user.ExpirationDate,
            UniqueCode = user.UniqueCode,
            BarcodeSvg = barcodeSvg
        };

        return View(model);
    }
    public IActionResult CardPending()
    {
        return View();
    }


    // Metoda privata pentru generarea codului de bare in format SVG
    private string GenerateBarcodeSvg(string uniqueCode)
    {
        var barcodeWriter = new BarcodeWriterSvg
        {
            Format = BarcodeFormat.CODE_128, // Tipul de cod de bare (de exemplu, CODE_128)
            Options = new ZXing.Common.EncodingOptions
            {
                Width = 800,
                Height = 180,
                Margin = 10,
                PureBarcode = true // FORȚEAZĂ codul să fie doar linii, fără text
            }
        };

        // Genereaza codul de bare pe baza uniqueCode
        var barcodeSvg = barcodeWriter.Write(uniqueCode);

        // Returneaza continutul SVG ca un string
        return barcodeSvg.Content;
    }
}
