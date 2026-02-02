# Rozsah a cíle

## Cíl přepisu
- Kompletní přepracování architektury do distribuovaného OOP programu.
- Desktop, Web a Mobile UI jsou nezávislé klienty.
- Služby a databáze mohou běžet na oddělených strojích.

## Hlavní schopnosti (must-have)
- Data acquisition (HTML i JSON/API)
- Normalizace a opravy dat (Memory + AI)
- Kategorie, brand, type, model, keywords, popisy
- Merge duplicit a variant
- Exporty a reporty (96 sloupců)
- Audit a validace memory

## Datove zdroje (referencni)
- Vzorove XLS exporty kompatibilni se specifikaci jzshop:
  `L:\Můj disk\XLS\Desaka Data\Exporty\{DD.MM.YY or DD.MM.YYYY}\*.xls`
- Tento adresar slouzi jen jako referencni specifikace; runtime ho nepouziva.
- Některe znacky budou mit XLSX/CSV ceniky (format zatim neznaly).
- Cenik slouzi jako zdroj ceny a filtru (které produkty ano/ne).
- Pouziti ceniku bude konfigurovatelne per eshop (povinnost zatim neurcena).
- Cenik musi jit nahrat uzivatelem na server (upload pres UI/API).
- Validace struktury ceniku: TBD (format zatim neznamy).
- Pincesobchod: JSON export, zpracovani odlisne od HTML downloaderu (detail pozdeji).

## Povinne vystupy
- XLS export kompatibilni se specifikaci jzshop.
- CSV export, ktery je primou kopii XLS (stejna data i struktura).
- Vystupni umisteni je uzivatelsky konfigurovatelne; vychozi hodnota odpovida soucasnemu projektu.
 
## Logovani a audit
- Povinny plny audit log (kdo/co/kdy/proc) vedle technickych logu.

## Notifikace
- Povinne in-app/proprietarni notifikace.
- Push notifikace jsou povinne (bez emailu).
- Push notifikace musi jit do desktop, web i mobil klientu.
- Typy notifikaci jsou konfigurovatelne (zapnout/vypnout).
- Vychozi udalosti: dokoncení downloadu, chyba downloadu, zmena z autopollingu, cekajici AI schvaleni, dokoncení unifier behu, chyba unifieru, export hotovy.

## Pristup a API
- API bude chraneno API klicem (bez prihlasovani).
- IP allowlist se nepouzije (kvuli pristupu z mobilu).
- API klic je natvrdo konfigurovany na serveru, uzivatel ho v UI nevidi a nespravuje.

## Role uzivatele a automatizace
- System ma byt maximalne automatizovany.
- Uzivatel primarne slouzi jako:
  - spoustec mimo plan
  - kontrolor vysledku
- UI priority pro uzivatele:
  - GUI pro kontrolu a spravu memory databaze
  - spousteni jednotlivych eshop downloaderu
  - konfiguracy stranky (jadro systemu)
  - kontrola progressu stahovani
  - kontrola AI doplnenych dat a schvalovani

## UI princip
- UI je jedno globalni a komplexni.
- Uzivatel nema vnimat separaci na vice backend sluzeb.

## Memory model (minimalni pozadavek)
- Memory zaznam obsahuje alespon: key, value, zdroj.
- Audit nad memory je povinny, ale resi se auditni funkci/logem, ne nutne strukturou memory tabulky.
- Import/export mezi DB a CSV musi byt podporovan.
- Jednotlive memory musi jit editovat pres GUI.
- Import z CSV je natvrdo prepis (overwrite) existujicich zaznamu, ale nic se nemaze.
- Jazykova vrstva memory musi pocitat s tim, ze uzivatel muze jazyk pridat i odebrat.
- Odebrani jazyka data nemaze; jazyk se zachova (deaktivace bez ztraty dat).
- Rozdeleni memory zachovat jako kombinaci:
  - global/shared (jazykove nezavisle ciselniky a konfigurace)
  - per-jazyk memory (name/desc/keywords/category mapping atd.)
- Multijazykovy export v jednom souboru se nepodporuje (export je per jazyk).
- Export ze snapshotu/konkretniho behu je volitelna schopnost (nice-to-have), ne bezpodminecna nutnost.

## AI klice
- AI API klice jsou uzivatelsky konfigurovatelne.
- Ulozeni musi byt sifrovane (kradez disku nesmi odhalit klic).

## AI provideri a dostupnost
- Podpora co nejvetsiho rozsahu AI provideru.
- System musi kontrolovat technicke limity serveru (disk, dostupnost API/sluzeb) a podle toho povolit/zakazat provider.
- Lze nastavit ruzne providery pro ruzne ulohy, ale pouze z dostupnych v danem case.
- Rezim auto-confirm AI vysledku bude zachovan (konfigurovatelny).
- Pocitat s batch AI zpracovanim, ale je nutne resit nepresnosti generovani.
- Sledovani AI nakladu je povinne a musi byt presne (soucasna verze je nedostacujici).
- Offline rezim: vsechny funkce, ktere mohou fungovat bez internetu, musi byt dostupne (network-dependent funkce se degraduji).

## Jazyky
- Jazykovy seznam je plne konfigurovatelny (povoleni/zakazani).
- Vychozi jazyk: cestina (CS).

## Paralelni behy
- Musi byt podporovany paralelni behy.
- Zadne sdileni mezi behy (oddeleny workspace, logy, data i vystupy).

## AI a uzivatelsky vstup
- AI je volitelna (offline rezim pouze z memory je povinny).
- Workflow obsahuje uzivatelske vstupy pri validaci/opravach (detail bude doplnen).

## Uzivatelske vstupy (soucasny system - reference)
- Vyber podobneho klice z memory (interaktivni volba z kandidatu).
- Rucni zadani:
  - Category ID
  - Brand code (3 znaky)
  - Category code (cislo)
  - Platformni category mapping (Heureka/Zbozi/Google/Glami)
- Potvrzovani AI navrhu (Enter = potvrdit, nebo prepsat hodnotu).
- Rucni zadani hodnot vlastnosti produktu (brand/type/model/category/desc/shortdesc/keywords).
- Standardizace variant (rucni vstup hodnot variant).
- Reseni konfliktu pri merge (vyber hodnoty z vice moznosti nebo vlastni vstup).
- Memory checker: rucni opravy/volby pri cisteni memory.

## Automaticke behy a workflow
- Automaticke behy se tykaji pouze stahovani (Acquisition).
- Unifier muze bezet automaticky, ale pri nutnosti uzivatelskeho schvaleni se job ulozi a ceka na interakci z UI.
- Stav jobu musi byt zjistitelny pres API.
- Planovani automatickych behu musi byt uzivatelsky konfigurovatelne (vcetne deaktivace).
- Eshopy se konfiguruji individualne (URL + casova okna/intervaly).
- Rucni spusteni jednotliveho eshopu musi byt mozne kdykoliv pres API/GUI.
- Rucni spusteni vzdy znamena plny beh (zadne test/validacni zkracene rezimy).
- Autopolling: system musi umet prubezne kontrolovat jednotlive produkty a detekovat zmeny.
- Autopolling:
  - bezi paralelne s plnymi downloady, ale nesmi bezet soucasne pro stejny eshop, pokud bezi jeho plny download
  - ma vlastni konfiguraci per eshop (oddelenou od plneho downloadu)
  - respektuje ItemFilter (polozky vyrazene z hlavniho toku se nekontroluji)
  - nesmi zasahovat do uz hotovych reportu/exportu
  - je nezavisly na velkem unifieru (cil: nevolat obri unifier kvuli jednotkam zmen)
  - pri detekci zmeny uklada do vlastni evidence; do hlavni tabulky zapisuje v batchi (napr. 1x denne)
  - soucasne vede oddelenou evidenci pro autopolling (autopoll log/store)
  - z autopollingu musi jit vytvaret XLS i CSV vystupy
  - autopolling respektuje format *Output.csv a porovnava stejne parametry jako tento soubor
  - identita produktu pro autopolling je URL
  - porovnani probiha proti aktualnimu stavu produktu se stejnou URL v DB
  - pri absolutni shode se zapisuje pouze textovy log, bez zmeny DB
  - pri neshode autopolling produkt aktualizuje (bez schvalovani)
  - schvalovani a inteligentni validace patri unifier modulu
  - autopolling je parciální downloader: jeho cil je webscrape do DB
  - autopolling vybira URL z DB; nove URL objevuji pouze plne downloadery
  - autopolling je masivne konfigurovatelny:
    - uzivatel muze pridat vlastni filtry podle kterehokoliv sloupce *Output.csv
    - ItemFilter nelze autopollingem prebit; blokovane polozky vyzaduji zmenu ItemFilteru
  - autopolling je rule-based (jako firewall):
    - uzivatel pridava "seance/pravidla" s definici co kontrolovat a jak casto
    - pravidla umi casova okna, frekvenci a filtry (napr. znacka Yasaka, 1 produkt/hod, 06:00-24:00)
  - pravidla se vyhodnocuji paralelne a navzajem se ignoruji
  - autopolling se vynecha, pokud bezi hlavni downloader proti stejne bazove URL/eshopu
  - autopolling je samostatny modul/sluzba

## Zásadní omezení
- REST jako primární komunikační rozhraní
- Event bus pro dlouhé joby
- Docker-ready, ale ne Docker-dependent
- MSSQL dominantní provider, DB vyměnitelná
