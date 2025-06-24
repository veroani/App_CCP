using App_CCP.Data;
using App_CCP.Models;
using App_CCP.Services;
using App_CCP.Services.Config;
using App_CCP.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static App_CCP.Controllers.AccountController;


var builder = WebApplication.CreateBuilder(args);

// Incarca fisierele de configurare, inclusiv cele locale (daca exista)
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Seteaza cultura aplicatiei
var cultureInfo = new CultureInfo("ro-RO");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Adauga servicii MVC si TempData
builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();
builder.Services.AddRazorPages();
builder.Services.AddSession();

// Configurare DBContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Identity & autentificare
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Injectie dependente
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<Users>, CustomClaimsPrincipalFactory>();

// EmailSettings (safe loading & validation)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("EmailSettings"))
    .ValidateDataAnnotations()
    .Validate(s => !string.IsNullOrWhiteSpace(s.FromEmail), "FromEmail must be configured.");

builder.Services.AddTransient<ICustomEmailSender, CustomEmailSender>();

// Logging
builder.Logging.AddConsole();

var app = builder.Build();

// Seed initial baza de date (daca e cazul)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DatabaseSeeder.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Pipeline middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();


