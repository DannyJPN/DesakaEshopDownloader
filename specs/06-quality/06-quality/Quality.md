# Quality & testing (navrh)

## Strategie testu
- Unit testy pro jadro (Memory, Parsing, Comparation, Export/Import).
- Integration testy pro DataAccess (DB provider MSSQL/SQLite).
- E2E testy pro klíčové workflow (WebScrape -> Unifier -> Export).
- Testy pro validaci memory heuristik (must-pass dataset).
- Memory validation bezi jako samostatny test projekt (integration test), spousteny spolu s unit/integration testy.
- Memory validation nevykonava opravy; pouze generuje report.
 - Povinne kontroly (report):
   - ProductBrandMemory vs BrandCodeList
   - ProductType/Model neobsahuji Brand
   - ProductType/Model neobsahuji VariantName/VariantValue
   - Overlap ProductType vs ProductModel (konflikt stejne hodnoty)
   - Keywords format (Google/Zbozi)
   - ProductModelMemory vs Pincesobchod nazvy
   - NameMemory: bez testu (kryje DB trigger)
   - Unique values kontrola
 - Invalid value format kontrola se nepouziva (disabled).
 - Performance a zatezove testy.
 - UI testy (automatizovane).
- Security testy: black box + white box, pen-test skripty pro zname zranitelnosti.
- Bezpecnostni kategorie testu (povinne):
  - OWASP testy (black-box API)
  - SAST (staticka analyza kodu)
  - SCA (audit zavislosti/CVE)
  - Secrets scanning (uniky klicu)
  - Dependency audit
- Regresni testy (automaticky spoustene).
 - Cilem je 100% pokryti tam, kde je test technicky mozny.

## Akceptacni kritéria
- Export CSV/XLS musi byt obsahove shodny se specifikaci JZSHOP (sloupce + hodnoty).
- Autopoll nesmi spustit kontrolu pri bezicim WebScrape na stejny eshop.
- Unifier nesmi exportovat bez dokoncenych approvals.
- AI volani musi byt presne logovana (cena + provider + model).
 - Jakykoli fail testu blokuje release (no-go).

## CI/CD (navrh)
- Build + test pipeline (povinne).
- Release pipeline s verzovanim (povinne; bez zeleneho buildu nelze vydat).
- GitHub Copilot muze generovat unit testy, ale implementator je musi overit a doplnit.
- Security testy jsou povinny gate (blokuje release).
 - CI (kazdy commit): unit + integration + UI + regression testy.
 - CI (denne): security + performance testy.

## Test data
- Vzorove XLS z L:\Muj disk\XLS\Desaka Data\Exporty slouzi pouze jako referencni specifikace sloupcu.
- System tyto soubory v runtime nepouziva.
