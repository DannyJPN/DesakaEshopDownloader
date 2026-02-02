# UI technologie (volba + odvodneni)

## Volba
- Desktop + Mobile: .NET MAUI (samostatna UI aplikace).
- Web: ASP.NET Core Blazor Web App (samostatna web UI aplikace).
- Vsechny UI komunikují pouze pres REST/SSE/WS s backend sluzbami.

## Odvodneni
- Jednotny jazyk (C#) a sdilene DTO/validacni logika.
- MAUI pokryje desktop i mobil bez nutnosti dvou ruznych kodu.
- Blazor Web App umozni rychlou tvorbu web UI s dobrou integraci na .NET API.
- UI vrstvy jsou oddelene od core sluzeb a lze je nasadit na jine stroje.
- Podpora enterprise standardu (testovatelnost, DI, logging, konfigurace, CI/CD).

## Integracni pravidla
- UI nesmi pristupovat primo k DB ani file storage.
- UI pouziva pouze API (REST + SSE/WS pro progress).
- API klic je interni a v UI neni editovatelny.

## Rizika a mitigace
- MAUI maturita a vykon: potvrdit prototypem klíčové obrazovky (Memory editor, Approval UI).
- Web + MAUI sdileni komponent: jen sdilene DTO a validace, UI vrstvy zustanou oddelene.
