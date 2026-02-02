# Security (navrh)

## API
- Interni API klic je pevne zabudovany v aplikaci, uzivatel ho nevidi a nemeni.
- Zadne prihlasovani uzivatelu.
- IP allowlist se nepouziva.

## AI klice
- AI API klice jsou v DB ulozene sifrovane (aplikacni sifrovani).
- Klice se nesmi dat ziskat pri kradezi disku.
- Decrypt pouze v runtime, v pameti.
- Sifrovaci klic neni v DB (bere se z secrets store / env).

## Pincesobchod API klice
- Stejny rezim jako AI klice (sifrovane v DB, klic v secrets store).

## Secrets management
- Produkce: environment secrets nebo OS key vault.
- Dev: local user secret store.
- Rotace klicu podporovana (manualni).

## Citliva data
- Logy nesmi obsahovat plne API klice.
- Audit log smi obsahovat identifikatory, ne tajemstvi.
- Import/export soubory s klici jen pres sifrovany format (pokud bude potreba).

## Runtime
- Sluzby pouzivaji runtime_lock (heartbeat + expiry) pro koordinaci.
- Kazda sluzba ma vlastni logy.
