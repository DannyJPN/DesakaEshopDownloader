# Architektura systému

## Cílový provozní model (nejmene menitelne)
- Vsechny UI (desktop/web/mobile), databaze, datova vrstva i sluzby mohou bezet na oddelenych strojich.
- UI klienti nikdy nepristupuji primo k DB, pouze pres REST API.
- Dlouhotrvajici workflow jsou asynchronni a ridena event busem.
- Docker-ready, ale ne Docker-dependent (kazda sluzba musi jit spustit i bez kontejneru).

## Principy
- Clean/Hexagonal Architecture
- SOLID + DIP (core zavisi jen na abstrakcich)
- SOA / coarse-grained services (ne mikroservisy), distribuovane sluzby v runtime

## Základní vrstvy (v kazde sluzbe)
- Domain: entity a pravidla
- Application: use-cases, workflow, orchestrace
- Infrastructure: DB, AI, HTTP, FS, export
- Presentation: REST API (zadne UI) + Swagger/OpenAPI pro samostatne testovani

## Topologie sluzeb
- WebScrapeService: stahovani a extrakce dat
- AutopollingService: prubezne kontroly produktu
- Unifier: sjednoceni, dopocet, export
- MemoryService: memory CRUD a hledani
- ConfigService: nastaveni, ciselniky, klice
- AIService: AI inference, retry, rate-limit (schvaleni v UI)
- ValidationService: test tlacitka a validace konfigurace
 - NotificationService: notifikace do UI (bez persistence)
 - Orchestrator: zatim ne (decentralizovane schedulery + runtime_lock)

## Komunikace
- Primarne REST (sync).
- Event bus pro asynchronni kroky (dlouhe joby).
