# Numele Proiectului
  Aplicație web pentru emiterea unui card cultural

## Livrabilele proiectului
- Codul sursă complet.
- Instrucțiuni pentru compilarea aplicației.
- Instrucțiuni pentru instalarea și lansarea aplicației.

## Adresa repository-ului
[https://github.com/veroani/App_CCP/]

## Compilare
Clonarea proiectului > obținerea aplicației din sursă și pregătirea ei pentru rulare:
git clone https://github.com/veroani/App_CCP.git
cd App_CCP
Restaurarea pachetelor NuGet > descarcă toate dependințele (ex: EntityFramework, Identity):
dotnet restore
Compilarea soluției > verifică dacă toate fișierele C# sunt valide și compilabile:
dotnet build

## Instalare (Configurări inițiale și setup al bazei de date)
Configurarea appsettings.json > Setează stringul de conexiune la baza de date SQL Server:
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CardCulturalDB;Trusted_Connection=True;"
}
Crearea bazei de date prin migrații > Creează toate tabelele definite prin modele și migrații EF Core:
dotnet ef database update

## Rulare
Rulare locală > Aplicația va porni pe https://localhost:7237 (sau alt port definit) :
dotnet run

## Observații 
Pentru email, e nevoie de configurare SMTP în appsettings.json.
Aplicația poate fi compilată și rulată local prin .NET CLI sau Visual Studio. 
După clonarea din GitHub și restaurarea pachetelor, baza de date este creată automat prin migrații EF Core. 
Lansarea aplicației se face prin dotnet run, aceasta fiind accesibilă pe localhost într-un browser. Configurația SMTP și stringul de conexiune pot fi ajustate în appsettings.json.
În scopul testării locale, fișierul appsettings.json este inclus în proiectul privat de pe GitHub. În cazul publicării aplicației sau colaborării externe, se va exclude din repo și se va folosi Secret Manager sau variabile de mediu.
