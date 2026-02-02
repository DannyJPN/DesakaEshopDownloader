# API rozhrani (navrh)

## Obecne
- REST + JSON je povinne pro vsechny sluzby.
- Swagger/OpenAPI je povinne pro vsechny REST sluzby v debug/dev modu.
- V produkci Swagger vypnut.
- Autentizace: pevny interni API klic, uzivatel ho nevidi.
- WebSocket/SSE/polling: volitelne pro progress/long-running operace (podpora predpokladana).
- API prefix: /api/v1
- Transport: HTTPS povinne pro citlive operace (API klice).

## Sluzby (REST)

### WebScrape
- /webscrape/start (POST)  // start downloadu eshopu
  - req: { eshop_id, force?: bool, correlation_id?: string }
  - resp: { run_id, status }
  - idempotentni per eshop (pokud bezi, vraci status)
- /webscrape/status (GET)  // stav beziciho jobu
  - query: run_id | eshop_id
  - resp: { run_id, status, progress, started_at, updated_at, message }
- /webscrape/logs (GET)    // log stream nebo batch
  - query: run_id, from?: timestamp
- /webscrape/list (GET)    // seznam eshopu / jobu
  - query: status?, from?, to?
- /webscrape/progress/stream (GET)  // SSE (volitelne)

### Autopoll
- /autopoll/start (POST)
  - req: { rule_id? | eshop_id?, correlation_id?: string }
  - resp: { run_id, status }
  - idempotentni per pravidlo (pokud bezi, vraci status)
- /autopoll/status (GET)
  - query: run_id | rule_id
  - resp: { run_id, status, progress, started_at, updated_at, message }
- /autopoll/rules (GET/POST/PUT/DELETE)
  - GET: list rules
  - POST: create
  - PUT: update
  - DELETE: delete
- /autopoll/logs (GET)
  - query: run_id, from?: timestamp
- /autopoll/batch-commit (POST)
  - req: { from?: timestamp, to?: timestamp }
  - resp: { batch_id, status }
- /autopoll/reports (GET)
  - query: date?

### Unifier
- /unifier/start (POST)
  - req: { run_webscrape_first?: bool, correlation_id?: string }
  - resp: { run_id, status }
- /unifier/status (GET)
  - query: run_id
  - resp: { run_id, status, progress, started_at, updated_at, message }
- /unifier/pending-approvals (GET)
  - resp: { items: [ { id, product_code, url, image_url, property_name, current_value, suggested_value } ] }
- /unifier/approve (POST)
  - req: { approval_id, action: approve|override|reject, value?: string }
  - resp: { status }  // dalsi polozka se nacita samostatne
- /unifier/logs (GET)
  - query: run_id, from?: timestamp

### Memory
- /memory/search (GET/POST)  // exact + fuzzy
  - req: { memory_type, query, language_code?, mode: exact|fuzzy }
  - resp: { items: [ { id, key, value, source, created_at, updated_at } ] }
- GET primarne pro exact match; POST jen pro slozite dotazy nebo create/update.
- /memory/import (POST)
  - req: { memory_type, file }  // CSV/XLS
  - resp: { imported_count, warnings }
- /memory/export (GET)
  - query: memory_type, language_code?
- /memory/entries (GET/POST/PUT/DELETE)
  - req: { memory_type, id?, key?, value?, source? }

### Config
- /config/eshops (GET/POST/PUT/DELETE)
  - req: { id?, name, base_url, is_enabled, download_schedule, price_list_source, language_flags? }
- /config/autopoll-rules (GET/POST/PUT/DELETE)
  - req: { id?, name, is_enabled, eshop_id, interval_value, interval_unit, window_start?, window_end?, filter_definition }
- /config/paths (GET/POST)
  - req: { paths: { output_root, raw_root, images_root, reports_root, logs_root, autopoll_reports_root, temp_root } }
- /config/languages (GET/POST/PUT)
  - req: { code, name, is_enabled, is_default }
- /config/ai (GET/POST/PUT)
  - req: { task_name, provider_id, model_name, api_key_plaintext, is_enabled }
- /config/validate (POST)  // test tlacítka
  - req: { type, payload }

### Notification
- /notify/push (POST)  // odeslani notifikace do UI
  - req: { event_type, title, message, severity, data?, correlation_id? }
  - resp: { status }
- /notify/status (GET)
  - resp: { enabled, channels }

## Poznamky
- Detaily request/response budou doplneny po analyzach stavajicich skriptu.
