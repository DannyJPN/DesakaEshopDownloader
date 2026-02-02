# Služby a odpovědnosti

## WebScrapeService (Full Download)
- Orchestrace, spusteni a provedeni jednotlivych downloaderu.
- Uklada RAW HTML/JSON a pred zapisem porovnava hash se starsim souborem.
- Pokud je hash stejny, preskoci se pouze zapis na disk; soubor se stejne nacte a parsuje.
- Obrázky uklada a do DB uklada cestu k souboru.
- Uloziste RAW/obrazku je konfigurovatelne (serverovy disk nebo klientsky disk).
- ItemFilter se zde neaplikuje (to dela unifier).
- Extractory jsou soucasti WebScrapeService; extractor je unikatni per eshop.
- WebScrapeService primarne zapisuje do DB.
- Soubory (CSV/XLS) generuje jen pokud je to povoleno v konfiguraci nebo manualne spusteno v UI.
- Podporuje paralelni beh vice eshopu.
- Extractor = parser HTML/JSON do standardizovaneho objektu.
- Rate limiting a retry je na urovni jednotlivych scraperu.
- WebScrapeService podporuje HTML i JSON/API scraper; JSON je zatim jen pro pincesobchod.
- Detailni logovani prubehu per eshop (pocty kategorii/produktu/obrazku).
- User-Agent je konfigurovatelny (globalne i per eshop) - nizka priorita.
- Proxy podpora: neni k dispozici (vyzaduje vlastni proxy), TOR protokol jako nizka priorita.
- Anti-bot/captcha strategie zatim neni pozadovana.
- Planovani: interval + volitelne casove okno (mimo spicku).
- Retry politika:
  - docasne chyby: exponencialni backoff, 10 pokusu
  - trvale chyby: bez retry
  - kriticke chyby: ukoncit job
- Paralelismus: globalni limit soubeznych eshopu, konfigurovatelny.

## AutopollingService
- Samostatny projekt/sluzba (oddeleny od WebScrapeService).
- Musi pouzivat totozny extractor jako WebScrapeService.
- Uklada stejna RAW data a obrazky jako WebScrapeService.
- WebScrapeService = hromadny scrape celych eshopu.
- AutopollingService = scrape jednotlivych produktu.
- Kazda sluzba (samostatny projekt) ma vlastni logy.
- Autopoll pravidlo musi obsahovat: eshop + interval (vychozi 1 produkt za hodinu).
- Volitelne: casovy rozsah, kdy ma bezet (kvuli pracovni dobe eshopu).
- Filtry jsou soucasti pravidla.
- Filtry jsou standardni "excel-like" pro kazdy sloupec *Output.csv / DB.
- ItemFilter se aplikuje automaticky a nelze ho vypnout (lze jen upravit v ConfigService).
- Paralelismus je na urovni pravidel (pravidla bezí soubezne).
- Jedno pravidlo nepouziva vnitrni paralelismus (kontroluje jednotky produktu).
- Vyber dalsiho produktu: round-robin podle posledni kontroly.
- Pravidla maji pouze Enable/Disable (bez specialniho pause stavu).
- Autopoll generuje stejny typ reportu jako WebScrape (CSV+XLS), rizen konfiguraci.

## ScrapingLibrary (shared)
- Hloupá sdilena knihovna s extractory (HTML/JSON parsovani).
- Neni samostatne spustitelna.

## DataAccessLibrary (shared)
- Sdilena knihovna pro praci s databazi (DataService) a soubory na disku (FileService).
- Kontrola/zakladani slozek se dela pred kazdou akci, ktera je potrebuje (FileService).
- Samostatna knihovna pro DB pristup.
- Kazda tabulka ma svoje rozhrani a svuj kontext.
- Sluzby mohou vracet kombinovana data.
 - FileService resi pouze IO proti disku (bez znalosti obsahu/formatu domeny).
 - Validuje zapis podle pripony a podporuje: HTML, JSON, JPG, PNG, BMP, TXT (logy jsou TXT i s .log).
- Vyssi formaty (snapshot, CSV schema apod.) resi Export/ImportService; FileService jen zapisuje data do zvoleneho formatu.
- Komprese (ZIP) neni pozadovana.

## ComparationLibrary (shared)
- Sdilena knihovna pro porovnavani (autopolling shoda/neshoda a dalsi srovnavace).

## ExportLibrary (shared)
- Sdilena knihovna pro generovani standardnich vystupu.
- WebScrapeService i AutopollingService produkuji stejny typ souboru (Autopoll casto s malym poctem polozek).
- Format JZSHOP bude soucasti ExportLibrary.

## ExportService (shared logic)
- Prevod z objektu/seznamu objektu do spravneho souboroveho formatu.
- Vola FileService pro zapis na disk.
- Podporovane formaty: CSV, XLS, XLSX (UTF-8).

## File access (UI)
- UI pristupuje k RAW/obrazkum/exportum/reportum pouze pres API download (ne pres sdilenou slozku).

## ImportService (shared logic)
- Opačny proces k ExportService (soubor -> objekty).
- Podporovane formaty: CSV, XLS, XLSX (UTF-8).

## Unifier (sluzba/modul - rozsekani bude upresneno)
- Sjednocuje vystupy eshopu do jednoho standardizovaneho vystupu.
- Dopocitava chybejici vlastnosti a standardizuje je.
- Cilem je maximalni presnost a minimalni potreba uzivatelske intervence.
- Nevyvolava WebScrapeService primarne.
- V UI bude volba "Spustit stahovani pred aktivaci unifikace":
  - spusti WebScrapeService a unifier ceka na signal dokonceni.
- Pri samostatnem spusteni unifikace zpracuje aktualni stav "Output" casti DB.
- Pokud neni dostupna DB, unifier hleda Output soubory stavajicim zpusobem (fallback).
- Unifier ma vlastni planovac.
- Unifier se nespusti, pokud bezi libovolny webscraper.
- Pri cekani na schvaleni pokracuje na ostatnich produktech a vytvari pending queue.
- Exportni faze se nespusti, dokud nejsou vsechna schvaleni vyrizena.
- Po doruceni schvaleni unifier pokracuje z ulozeneho stavu a dokonci export.

## UnifyingLibrary (shared)
- Obsahuje data-holding tridy (napr. DownloadedProduct).
- Obsahuje servisni tridy pro transformace (napr. CSV -> DownloadedProduct).

## NameMemory (DB)
- NameMemory bude zachovana.
- Aktualizace bude resena DB triggerem (odvozeno z type/brand/model).
- Workflow zustava jako ve stavajicim systemu.

## Unifier - Repair/Normalization (internal)
- Service trida uvnitr Unifieru.
- Vola externi sluzby/knihovny: AI, Memory/DB, GUI (uzivatelske schvaleni).
- V offline/memory-only rezimu se AI kroky presouvaji na uzivatele (UI schvaleni).
- Povinny deterministicky extractor modul:
  - Implementovan z obsahu stavajicich memory souboru.
  - Musi vracet spravnou hodnotu pro kazdy soucasny zaznam memory souboru.
  - Musi existovat test; pokud test neprojde, modul se nesmi pouzit.
- Tento modul doplnuje (nenahrazuje) stavajici heuristiku; je to pokrocila heuristika.
- Poradi rozhodovani pro vlastnost:
  1) exact memory match
  2) heuristika (text match)
  3) deterministicky extractor
  4) AI
  5) uzivatel
- AI lze globalne vypnout; ostatni kroky ne.

## Unifier - Merge (internal)
- Service trida uvnitr Unifieru (deduplikace a merge).
- Reseni konfliktu pouze interaktivne v UI (bez auto-pravidel).

## Unifier - Filtering (internal)
- Service trida uvnitr Unifieru (ItemFilter a dalsi filtry).

## Unifier - Export DTO Mapping (internal)
- Prevod seznamu DTO1 -> DTO2 (RepairedProduct -> ExportProduct).
- Nevi nic o disku ani XLS/CSV.

## Unifier - ProductCombiner (internal)
- Kombinace exportu z JSON a z unifieru.
- Service trida uvnitr Unifieru, muze volat dalsi sluzby.

## Unifier - Final Export (internal)
- Unifier vola ExportLibrary pro generovani jzshop XLS/CSV.

## MemoryService
- Zodpoveda za nacitani/validaci SupportedLanguages, Memory a EshopList.
- Memory/konfiguracni data jsou primarne v DB (ne v souborech).
- MemoryService funguje jako adapter mezi DataAccessService a sluzbami, ktere potrebuji "memory soubory".
- MemoryService vraci pouze filtrované subsety (ne cely blok), s push-down filtrováním do DataAccessService.
- MemoryService udrzuje ekvivalenty stavajicich "memory souboru".
- Poskytuje data tak, aby volajici nemuseli resit, zda zdroj je DB nebo soubory.
- Import/export do CSV obsluhuji samostatne sluzby, MemoryService je vola.
- Podporuje vyhledani:
  - exact match podle klice
  - podobne klice na zaklade lingvisticke podobnosti
- Lingvisticka podobnost bezi na strane serveru jako samostatna Service trida.
- MemoryService muze cacheovat vysledky vyhledavani (vykon).
- Hromadne operace jsou podporovane skrze import (CSV/XLS) jako jediny bulk kanal.

## ValidationService
- Validuje konfigurace (dostupnost URL, funkcnost API klicu, spojeni).
- Slouzi jen pro uzivatelske overeni v konfiguraci (napr. tlacitko TEST).
- Ostrou validaci provadeji rutiny, ktere spojeni pouzivaji (vypis messageboxu pri chybe).

## MemoryValidation (separatni)
- Samostatny modul pro validaci memory konzistence (ekvivalent memory_checker).

## Notification (mechanismus/sluzba)
- UI resi zobrazeni notifikaci.
- Samostatny mechanismus/sluzba detekuje udalosti a odpaluje notifikace.
- Notifikace bude samostatny projekt: odbera udalosti a posila push.
- Notifikacni sluzba neuklada notifikace do DB (bez perzistence).
- Doruceni je live: klient si data vyzada (GET) a UI zobrazi notifikaci.
- Primarni doruceni: WebSocket/SSE, fallback polling.
- Desktop/Web: WebSocket/SSE.
- Mobile: platformni push (FCM/APNS).

## Naming pravidlo (projekty)
- Nazvy projektu nesmi obsahovat "Service" (rezervovano pro nazvy trid).

## ConfigService
- Sluzba pro nastavovani (Settings) vseho, co je uzivatelsky konfigurovatelne.
- Spravuje mimo jine:
  - pevne jazykove nezavisle slovniky (napr. mapovani kategorii na ID)
  - seznam eshopu (nazev, URL, planovani)
  - pravidla autopollingu
  - AI provider/model pro jednotlive ulohy
  - cesty ke vsechným souborovym ulozistim
  - zapinani/vypinani jazyku
- Pevne slovniky musi byt editovatelne v GUI (ekvivalent).
- Konfigurace eshopu musi jit importovat i exportovat.
- Konfigurace cest v GUI pres standardni file/folder dialog.
- ConfigService spravuje AI API klice (server/klient API klic je vestaveny a nemenitelny).
- ConfigService spravuje API klice pro pincesobchod per jazyk.
- Konfigurace notifikacnich politik (zapnout/vypnout typy udalosti) je soucasti ConfigService.
- Validace konfiguraci je v GUI, ale provadi ji samostatna validacni sluzba.
- ItemFilter zachovat ve stavajicim CSV formatu (zatim beze zmen).

## AIService
- Zajistuje komunikaci mezi systemem a AI providerem (inference, batch, retry, rate-limit).
- Schvalovani AI vysledku je UI workflow (ne v AIService).
- Cekajici schvaleni musi byt ulozena v DB (prezije restart, sdilene mezi klienty).
- AIService zaznamenava presny cost per request (aktualni ceny).
- AIService udrzuje aktualni pricing (auto-fetch z API/webu, fallback manualni).
- Pricing se aktualizuje automaticky 1x mesicne.
- Pokud update selze, pouzije se posledni znama hodnota.
- Vychozi pricing je natvrdo inicializovan implementatorem.
- Pri selhani update se odesle notifikace.
- Fine-tuning je samostatna funkcionalita AIService (ne soucast Unifieru).

## AI Approval UI (pozadavek)
- Samostatna obrazovka s detailnim zobrazenim produktu (foto, URL).
- Zobrazeni konkretni vlastnosti k potvrzeni.
- Akce: potvrdit / opravit.
- Schvalovani pouze po jedne polozce (bez batch).
