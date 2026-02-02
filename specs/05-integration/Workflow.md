# Workflow a datove toky

## WebScrape -> DB
- WebScrapeService stahuje RAW HTML/JSON.
- Hash porovnani rozhoduje o zapisu na disk (parsing se dela vzdy).
- Vysledky se ukladaji do products_current a raw metadata.
- Soubory CSV/XLS se generuji jen pokud je to povoleno v konfiguraci nebo manualne.
- Downloader Output (nahrada Output.csv) se uklada do downloaded_* tabulek (bez JSON v DB).
 - Mapovani DB -> DownloadedProduct:
   - downloaded_product -> DownloadedProduct (name/desc/url)
   - downloaded_gallery -> gallery_filepaths (pipe-join)
   - downloaded_variant + downloaded_variant_option -> DownloadedProduct.variants

## Autopoll -> mezisklad -> DB
- AutopollingService stahuje jednotlive produkty (podle pravidel).
- Zmeny se ukladaji do autopoll_snapshot + autopoll_change_log.
- Batch commit (napr. 1x denne) zapisuje zmeny do products_current.
- Autopoll reporty (CSV+XLS) se generuji z meziskladu.

## Unifier
- Unifier zpracuje aktualni stav Output/DB.
- Fallback: pokud DB neni dostupna, unifier hleda Output soubory.
- Unifier se nespusti, pokud bezi WebScrape pro eshop s aktivnim schedulerem.
- Manualne spustene eshopy bez scheduleru unifier neblokuje.
- Approval workflow blokuje export, dokud nejsou vsechny polozky schvaleny.
- Fine-tuning neni soucast Unifieru (resi AIService samostatne).
- Unifier generuje reporty "vse" i "jen nove".
- ItemFilter se aplikuje az po oprave produktu (Repair/Normalization).
- "UnifiedProducts" CSV (backward compatibility) se negeneruje (zruseno).

## Repair/Normalization (detail)
- Poradi vyhodnoceni vlastnosti:
  1) exact memory match
  2) fuzzy match (doladi se dle unit testu)
  3) heuristika (text match)
  4) deterministicky extractor (pravidlovy modul)
  5) AI
  6) uzivatel
- Deterministicky extractor je samostatny modul, musi projit testy na vsech stavajicich memory zaznamech, jinak se nepouzije.
- Auto-confirm = AI vysledek jde rovnou do vystupu i do memory (source=AI).

## Merge (detail)
- String vlastnosti se slucuji pres "|" (duplicitni hodnoty odstranit).
- Konflikty se resi vzdy uzivatelsky (bez auto-pravidel).
- Varianta je duplicitni, pokud shodna key_value_pairs + current_price + basic_price + stock_status.

## Filtering (ItemFilter + Wrongs)
- ItemFilter se aplikuje po Repair/Normalization a po Merge.
- Odmitnuti pres ItemFilter se loguje (bez ukladani do Wrongs).
- Wrongs slouzi jako manualni seznam vylouceni jednotlivych produktu.

## Eventy
- Event bus pouzity pro notifikace (povinne).
- Dalsi orchestrace zatim neni; koordinace je decentralizovana.

## ProductCombiner (poznamka)
- Detailni logika price-change reportu je TBD.
- Implementator musi prostudovat aktualni kod + exportni soubory a ridit se realnym formatem.

## Approval workflow (detail)
- Approval item obsahuje: product_code, url, image_url, property_name, current_value, suggested_value, language.
- Schvaleni uklada hodnotu do Memory (source=USER); AI navrh je oznacen source=AI.
- Approval queue musi byt perzistentni a sdilena mezi klienty.

## Export & reporty
- Reporty: Report-all, Report-new + detailni reporty (typ/znacka/kategorie/combos).
- Formaty: CSV + XLS + XLSX.
- Ulozeni do timestampovane slozky (Reporty_YYYYMMDD_HHMMSS).
