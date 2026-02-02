# UI navrh (workflow + obrazovky)

## Principy
- Uživatel vidi jeden uceleny system (skryte sluzby).
- UI je hlavni misto pro konfiguraci, rucni spousteni a kontrolu.
- Vsechny notifikace se objevuji v UI (povinne).
- Progress dlouhych akci je viditelny (polling nebo SSE/WS).

## Hlavni obrazovky

### 1) Dashboard (Home)
- Stav sluzeb: WebScrape, Autopoll, Unifier, Memory, AI.
- Globalni notifikace + aktivni upozorneni.
- Tlacitka: Start WebScrape / Start Autopoll / Start Unifier.
- Prehled poslednich behu a chyb.

### 2) Konfigurace
- Eshopy (CRUD, URL, povoleni, schedule).
- Autopoll pravidla (CRUD, interval, casove okno, filtry).
- Cesty (output/raw/images/reports/logs).
- AI nastaveni (provider, model, klic).
- Notifikace (kanaly, eventy).
- Jazyky (enable/disable, default).
- Tlacitka "TEST" pro validaci (URL, API klic).

### 3) Memory
- Seznam typu memory (tabulky).
- Vyhledavani (exact + fuzzy).
- Editace zaznamu (GUI, po jednom; bulk edit ne).
- Import/Export CSV/XLS (rozsireny format se source+timestamps).
- Audit pro memory (zobrazeni historie zmen).

### 4) Autopoll
- Seznam pravidel + stav posledniho behu.
- Progress per pravidlo.
- Prehled denniho reportu (CSV/XLS) + download.
- Zobrazeni zmen (snapshot log) s filtrovani v UI (eshop/sloupec).

### 5) WebScrape
- Spusteni downloadu per eshop.
- Progress + logy.
- Prehled RAW souboru (metadata + hash) + thumbnaaily obrazku.

### 6) Unifier
- Spusteni unifikace.
- Stav a progress.
- Approval queue:
  - obrazovka s produktem, fotkou, URL, property, navrh AI
  - akce: approve / override / reject (po jedne, bez batch)

### 7) Logy
- Centralni zobrazeni logu (filtry podle sluzby).
- Export logu.

## UI workflow (zkracene)
- Uvodni konfigurace -> Nastaveni eshopu -> Nastaveni pravidel -> Start webscrape -> Unifier -> Export.
- Autopoll bezi na pozadi, generuje denni reporty.
- Uzivatelske schvalovani je blokujici pro export.
