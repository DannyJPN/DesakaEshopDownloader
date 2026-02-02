# Agent Access & Permissions (AI implementator)

## Root
- Spec root: `specs_architecture/` (bude kopirovano jako celek).

## Codex konfigurace
- Codex config: `specs_architecture/.codex/config.toml` (nastaveno na `H:\Programs\C#\DesakaEshopDownloader`).
 
## Claude konfigurace
- Claude config: `specs_architecture/.claude/settings.json`
- Claude local config: `specs_architecture/.claude/settings.local.json`

## Dostupne prostredky (zajisti uzivatel)
- RabbitMQ: uzivatel zajisti/spusti a doda pristup.
- Databaze: uzivatel zajisti/spusti a doda pristup.
- Secret Store: spravuje uzivatel (AI klice + interni API klic).
- Referencni XLS: `L:\Muj disk\XLS\Desaka Data\Exporty` (read-only).

## Databaze a migrace
- Primarne SQL skripty (ne EF migration runner).
- V repu bude specialni slozka pro SQL skripty (nazev TBD).
- Zmeny v DB delam pouze na zaklade dodaneho connection stringu.
- Pokud bude z DB mozno vygenerovat connection string, pouziju ho.

## Soubory a repo
- Plny read/write pristup v repo.
- Vytvareni novych projektu/slozek je povoleno.
- Instalace NuGet balicku je povolena.

## Provoz a build
- Spousteni dotnet build/test povoleno.
- Build UI (MAUI/Blazor) povoleny.
- Code signing: vyzaduje dodane certifikaty (spravuje uzivatel).

## Omezena prava (neposkytovat)
- Zadny pristup k Secret Store (krome hodnot, ktere uzivatel vlozi).
- Zadny pristup k produkcnim klicum mimo dev/test.
- Zadny pristup mimo repo bez explicitniho souhlasu.

## Ocekavane vstupy od uzivatele
- Connection string pro DB (dev/test).
- Pristup k RabbitMQ (URL/credentials).
- Reference ciselniku cen a cenikovych souboru (az budou známe).
