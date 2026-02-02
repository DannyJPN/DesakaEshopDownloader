# Deployment & provoz

## Nasazeni (nejmene menitelne)
- Kazda sluzba nasaditelna samostatne na vlastnim stroji.
- UI klienti (desktop/web/mobile) nasazeni nezavisle na sluzbach.
- DB bezi samostatne; sluzby k ni pristupuji pres provider vrstvu.
- Docker-ready, ale neni povinny. Bare-metal je plnohodnotny.
- Kazda sluzba bezi jako samostatny proces (worker/daemon), spustitelny jako OS sluzba nebo container.
- Service discovery: staticke adresy v configu (default).
- Health checks pro kazdou sluzbu (GET /health).
- Logy do konzole + souboru; centralni log server je volitelny, ale lokalni logy jsou povinne.

## Konfigurace a prostredi
- Dev / Staging / Prod
- Konfigurace pres appsettings + secrets store
- Centrala pro connection strings a API keys

## DB provoz
- MSSQL jako primarni storage v prod.
- Vymenitelne providery (napr. SQLite pro dev).
