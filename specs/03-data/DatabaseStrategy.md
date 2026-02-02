# DB strategie

## Požadavek
- Vyměnitelné providery
- MSSQL dominantní

## Implementace
- IRepository<T>, IUnitOfWork
- Provider switch přes konfiguraci

## Povinné providery
- MSSQL (prod)
- SQLite (dev)
- Volitelně PostgreSQL

## Hierarchie pojmu (DB struktura)
1) DB instance (jedna primarni MSSQL)
2) DB schema (logicke rozdeleni podle funkce/sluzby)
3) Tabulky (napr. data.products, autopoll.rules, memory.entries)
4) DataAccessLibrary (rozhrani per tabulka + context)
5) Sluzby (pouzivaji svuj schema set)

## Autopolling data
- Autopolling nema vlastni DB instanci.
- Pouziva vlastni schema/tabulky pro mezivysledky, logy a stav (napr. autopoll.*).
- Zmeny se ukladaji do meziskladu a do hlavnich produktu se zapisuji batch (napr. 1x denne).

## Sdilena produktova data
- WebScrape, Autopolling i Unifier pracuji nad stejnymi produktovymi daty.
- DataAccess vrstva musi byt navrzena tak, aby sluzby nepoznaly, pokud by se DB pozdeji oddelily.

## Historie zmen produktu
- Povinna historizace zmen produktu (versioning).
- Historizuji se vsechny stahovane atributy (*Output.csv).
- Obrazky se nehistorizuji, pouze cesty a URL.
- Retence historie je konfigurovatelna; vychozi 1 rok.
- Historizace je resena aplikacne (sluzby zapisuji do history tabulek).

## Evidence output souboru
- V DB bude evidence output souboru (eshop, cas, cesta k souboru) pouze pokud je soubor realne generovan (dle konfigurace/manualniho spusteni).

## Runtime stavy behu
- Sdileny stav behu bude ulozen v DB s TTL + heartbeat (detekce stale zámku).
