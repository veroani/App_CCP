# Aplicație web pentru emiterea unui card cultural

## Descriere generală
Aplicația își propune să automatizeze procesul de emitere și gestionare a unui card cultural, oferind utilizatorilor acces la parteneri, noutăți culturale și funcționalități administrative într-o platformă unificată.

## Structura livrabilelor
- Cod sursă complet în C# (ASP.NET Core 8 MVC)
- Structură organizată în:
  - `Controllers/` – logica de control a aplicației
  - `Models/` – clasele de model (date și entități)
  - `Views/` – fișierele Razor `.cshtml` pentru UI
  - `View_Models/` – modele de date pentru interfață
  - `Services/` – servicii pentru logica de business
  - `Data/` – contextul bazei de date
  - `Migrations/` – migrații EF Core pentru baza de date
  - `Validation/` – validatori custom
  - `TagHelpers/` – tag helpers personalizați pentru UI
  - `wwwroot/` – resurse statice (CSS, JS, imagini)
  - `Program.cs` – punctul de intrare al aplicației
  - `appsettings.json` – configurare aplicație
  - `.gitignore` – excluderea fișierelor binare și temporare
  - `Db_App_CCP_backup` – backup 
- Instrucțiuni pentru compilarea aplicației.
- Instrucțiuni pentru instalarea și lansarea aplicației.

## Adresa repository-ului
Codul sursă complet este disponibil la adresa: [https://github.com/veroani/App_CCP] (https://github.com/veroani/App_CCP)

## Compilare
- Instalare [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), Visual Studio 2022 sau Visual Studio Code
- Clonare proiect > obținerea aplicației din sursă și pregătirea ei pentru rulare
git clone https://github.com/veroani/App_CCP.git
cd App_CCP
- Restaurare pachete NuGet > descarcă toate dependințele (ex: EntityFramework, Identity)
dotnet restore
- Compilare a soluției > verifică dacă toate fișierele C# sunt valide și compilabile
dotnet build

## Instalare (Configurări inițiale și setup al bazei de date)
- Configurare appsettings.json > Setează stringul de conexiune la baza de date SQL Server
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CardCulturalDB;Trusted_Connection=True;"
}
- Creare a bazei de date prin migrări > Creează toate tabelele definite prin modele și migrări EF Core:
dotnet ef database update

## Rulare
Rulare locală > Aplicația va porni pe https://localhost:7237 (sau alt port pre definit) 
dotnet run

## Observații 
- Fișierele binare precum bin/, obj/ și fișierele de backup compilate NU sunt incluse în repository, conform .gitignore.
- Pentru email, e nevoie de configurare SMTP în appsettings.json.
- Aplicația poate fi compilată și rulată local prin Visual Studio. 
- După clonarea din GitHub și restaurarea pachetelor, baza de date este creată automat prin migrații EF Core. 
- Lansarea aplicației se face prin dotnet run, aceasta fiind accesibilă pe localhost într-un browser. Configurația SMTP și stringul de conexiune pot fi ajustate în appsettings.json.
  
În scopul testării locale, fișierul appsettings.json este inclus în proiectul privat de pe GitHub. În cazul publicării aplicației sau colaborării externe, se va exclude din repo și se va folosi Secret Manager sau variabile de mediu.
