# Dev Standards / Principles

## Struktura a vrstvy
- Kazda sluzba ma vrstvy Domain / Application / Infrastructure / Presentation.
- Vrstvy jsou organizovane jako slozky (ne samostatne projekty).

## Naming konvence
- Prefix namespaces/projektu: Desaka.*
- Suffixy vrstev: .Domain / .Application / .Infrastructure / .Api
- PascalCase pro nazvy (napr. Desaka.WebScrape.Application).

## Dependency pravidla
- Domain nema zavislosti mimo sebe.
- Application zavisi na Domain.
- Infrastructure zavisi na Application + Domain.
- Presentation (API) zavisi na Application.
- Sdilene knihovny (DTO/Contracts, utils) jsou povolene.
- Sluzby nesmi pristupovat do DB jinych sluzeb primo (pouze pres API/DataAccess).

## Error handling a spolehlivost
- Fail-fast na konfiguracni/IO chyby, ale sluzba nesmi "umrit" (timer scheduler bezi dal).
- Try/catch je povinny, ale kod ma byt psan tak, aby se chybam predchazelo (validace predem).
- Chyba jednoho produktu nikdy nezastavi job; pouze se zaloguje a pokracuje se dal.
- Kriticke chyby mohou ukoncit aktualni job, ale ne zastavit periodicky scheduler.

## Logging & observability
- Structured logging (strojove citelne, vyhledatelne).
- Correlation_id povinny pro kazdy job/run.
- U chyb logovat konkretni produkt (kod/URL) a duvod.

## Bezpecnost v kodu
- Aplikace musi byt "pevnost": maximalni mozne zabezpeceni.
- Povinne obrany:
  - ochrana proti CSV injection
  - path traversal kontrola u vsech souborovych cest
  - validace vstupu (delka, typ, zakazane znaky)
  - bezpecne zachazeni s HTML/JSON (sanitizace)
  - ochrana pred neplatnym kodovanim/encoding chybami
  - explicitni limity velikosti souboru a payloadu
  - audit a logging pro vsechny administrativni akce

## Security testing
- OWASP testy povinne.
- Testovat i externi infrastrukturu (napr. RabbitMQ konfigurace).

## Versioning & migrace
- DB schema zmeny povinne pres EF Core Migrations (verzovane v repu).
- API versioning povinny (/api/v1).
- Exportni format je stabilni (bez explicitni verze ve vystupu).

## Code review & branching
- PR review je povinne pred mergem do main.
- Main je chranena (zakazany prime push, vyjimka pouze init).
- Branching strategie: trunk-based (kratke feature branche).
 - Commity po logickych blocich (ne "vse najednou").
 - Commit zpravy popisne, vazane na konkretni zmenu.

## Coding style & linting
- Povinny formatter (napr. dotnet format) jako soucast CI.
- Povinny linting/staticka analyza jako gate pro merge.

## Dokumentace a ADR
- ADR (Architectural Decision Records) povinne pro zasadni rozhodnuti.

## Release management
- Verzionovani: SemVer (MAJOR.MINOR.PATCH).
 - Release pipeline produkuje artefakty pro sluzby i vsechny UI (desktop/mobile/web).
 - Code signing povinny pro desktop/mobile buildy.
