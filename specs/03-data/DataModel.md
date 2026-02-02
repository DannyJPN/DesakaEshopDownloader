# Datový model

## Klíčové entity
- Product, Variant
- MemoryEntry
- CategoryMapping
- Job, JobStep
- ExportRecord

## DB schema (navrh)
- data (produkty, varianty, aktualni stav)
- scrape (RAW metadata, output files)
- autopoll (pravidla, logy, zmeny)
- memory (memory entries)
- config (eshopy, jazyky, cesty, AI config)
- notify (tokeny zarizeni, kanaly)
- runtime (stavy behu s TTL)
- audit (audit log)

## Varianty (normalizace)
- Varianty jsou v samostatnych tabulkach (normalni formy).
- Doporuzeny model:
  - product_variant (vazba product_id + cena/stock per produkt)
  - variant_option (normalizovane key/value pary, vice kombinaci na produkt)
- Produkt bez variant ma implicitni "default" variantu s prazdnymi key_value_pairs.

## Produkty (aktualni stav + historie)
- Hlavni tabulka products_current odpovida Output.csv + rozsireni o:
  - url (identifikace eshopu pres domenu URL, bez eshop_id)
  - url_domain (normalizovana domena, plnena DB triggerem)
  - created_at
  - updated_at
  - is_active
- Output.csv nezahrnuje created_at/updated_at (datove sloupce nejsou soucast exportu).
- Historie je vedena v products_history jako SCD2 snapshot:
  - plna kopie produktu pri zmene
  - valid_from, valid_to
  - change_source (webscraper/autopoll/unifier)
- Primarni klic: id (identity)
- Unikatni constraint: kod (globalne, bez vazby na eshop)
- created_at/updated_at: UTC

## Varianty (vazba na produkt)
- Varianta je v unikatnim vlastnictvi produktu (1 varianta patri 1 produktu).
- product_variant nese cenu/dostupnost (a dalsi variantni atributy).
- V aktualnim kodu (desaka_unifier/unifierlib/variant.py) ma varianta tyto atributy:
  - key_value_pairs (dict)
  - current_price
  - basic_price
  - stock_status
  - variantcode
- variantcode je unikatni globalne a musi obsahovat kod produktu jako prefix
  (napr. varianta DEN01050004-02 patri produktu DEN01050004).
- Format variantcode (soucasny generator): base_code + "-" + 2 cislice (01, 02, ...).
- DB constrainty:
  - products_current: UNIQUE (id, kod)
  - product_variant: UNIQUE (variantcode)
- Vazbu variantcode -> product.kod validuje datova vrstva/ukladaci rutina (ne DB).
- Varianta reprezentuje kombinaci vice option_name/option_value (napr. barva+velikost).
- Pocet option par na variantu neni pevne omezen; export do JZSHOP mapuje max 3 pary (varianta1..3).

## Output.csv sloupce (zdroj: desaka_unifier/unifierlib/export_product.py)
- id
- typ
- varianta_id
- varianta1_nazev
- varianta1_hodnota
- varianta2_nazev
- varianta2_hodnota
- varianta3_nazev
- varianta3_hodnota
- varianta_stejne
- zobrazit
- archiv
- kod
- kod_vyrobku
- ean
- isbn
- nazev
- privlastek
- vyrobce
- dodavatel_id
- cena
- cena_bezna
- cena_nakupni
- recyklacni_poplatek
- dph
- sleva
- sleva_od
- sleva_do
- popis
- popis_strucny
- kosik
- home
- dostupnost
- doprava_zdarma
- dodaci_doba
- dodaci_doba_auto
- sklad
- na_sklade
- hmotnost
- delka
- jednotka
- odber_po
- odber_min
- odber_max
- pocet
- zaruka
- marze_dodavatel
- seo_titulek
- seo_popis
- eroticke
- pro_dospele
- slevovy_kupon
- darek_objednavka
- priorita
- poznamka
- dodavatel_kod
- stitky
- cena_dodavatel
- kategorie_id
- podobne
- prislusenstvi
- variantove
- zdarma
- sluzby
- rozsirujici_obsah
- zbozicz_skryt
- zbozicz_productname
- zbozicz_product
- zbozicz_cpc
- zbozicz_cpc_search
- zbozicz_kategorie
- zbozicz_stitek_0
- zbozicz_stitek_1
- zbozicz_extra
- heurekacz_skryt
- heurekacz_productname
- heurekacz_product
- heurekacz_cpc
- heurekacz_kategorie
- google_skryt
- google_kategorie
- google_stitek_0
- google_stitek_1
- google_stitek_2
- google_stitek_3
- google_stitek_4
- glami_skryt
- glami_kategorie
- glami_cpc
- glami_voucher
- glami_material
- glamisk_material
- sklad_umisteni
- sklad_minimalni
- sklad_optimalni
- sklad_maximalni

## Navrh tabulek (detail)

### products_current
- id (BIGINT IDENTITY, PK)
- kod (NVARCHAR(11), UNIQUE)
- url (NVARCHAR, NOT NULL)
- url_domain (NVARCHAR, NOT NULL)
- is_active (BIT, NOT NULL, default 1)
- created_at (DATETIME2, NOT NULL, default GETUTCDATE)
- updated_at (DATETIME2, NOT NULL, default GETUTCDATE)
- + vsechny sloupce z Output.csv (viz vyse)
- Indexy:
  - UNIQUE(kod)
  - INDEX(url_domain)
  - INDEX(url)
  - INDEX(updated_at)

### products_history (SCD2)
- id (BIGINT IDENTITY, PK)
- product_id (BIGINT, FK -> products_current.id)
- valid_from (DATETIME2, NOT NULL)
- valid_to (DATETIME2, NULL)
- change_source (NVARCHAR, NOT NULL)  // webscraper/autopoll/unifier
- snapshot_columns: plna kopie products_current (vsechny sloupce, vc. Output.csv, url, is_active)
- Indexy:
  - INDEX(product_id, valid_from DESC)
  - INDEX(valid_to)

### product_variant
- id (BIGINT IDENTITY, PK)
- product_id (BIGINT, FK -> products_current.id)
- variantcode (NVARCHAR(14), UNIQUE)
- current_price (DECIMAL(18,2))
- basic_price (DECIMAL(18,2))
- stock_status (NVARCHAR)
- Indexy:
  - INDEX(product_id)

### variant_option (normalizace variant)
- id (BIGINT IDENTITY, PK)
- variant_id (BIGINT, FK -> product_variant.id)
- option_name (NVARCHAR, NOT NULL)
- option_value (NVARCHAR, NOT NULL)
- UNIQUE(variant_id, option_name, option_value)

## Identita produktu
- Code (brand+category+seq)
- Hash pro detekci duplicit

## Scrape/Autopoll runtime + logy (navrh)

### scrape_run
- id (BIGINT IDENTITY, PK)
- eshop_id (INT, FK -> config.eshop.id)
- start_at (DATETIME2, NOT NULL)
- end_at (DATETIME2, NULL)
- status (NVARCHAR, NOT NULL)  // running/success/error/cancelled
- error_message (NVARCHAR, NULL)
- items_total (INT, NULL)
- items_changed (INT, NULL)
- items_skipped (INT, NULL)
- duration_ms (BIGINT, NULL)
- Indexy:
  - INDEX(eshop_id, start_at DESC)
  - INDEX(status)

### scrape_raw_metadata
- id (BIGINT IDENTITY, PK)
- eshop_id (INT, FK -> config.eshop.id)
- url (NVARCHAR, NOT NULL)
- file_path (NVARCHAR, NOT NULL)
- content_hash (NVARCHAR, NOT NULL)
- content_size (BIGINT, NULL)
- content_type (NVARCHAR, NOT NULL)  // html/json
- downloaded_at (DATETIME2, NOT NULL)
- Indexy:
  - INDEX(eshop_id, downloaded_at DESC)
  - INDEX(content_hash)
- Retence RAW/obrazku: bez automaticke retence (manualni udrzba).

## Downloader Output (nahrada Output.csv)

### downloaded_product
- id (BIGINT IDENTITY, PK)
- eshop_id (INT, FK -> config_eshop.id)
- name (NVARCHAR, NOT NULL)
- short_description (NVARCHAR, NULL)
- description (NVARCHAR(MAX), NULL)
- main_photo_path (NVARCHAR, NULL)
- url (NVARCHAR, NOT NULL)
- created_at (DATETIME2, NOT NULL, default GETUTCDATE)
- Indexy:
  - INDEX(eshop_id, created_at DESC)
  - INDEX(url)

### downloaded_gallery
- id (BIGINT IDENTITY, PK)
- product_id (BIGINT, FK -> downloaded_product.id)
- filepath (NVARCHAR, NOT NULL)
- Indexy:
  - INDEX(product_id)

### downloaded_variant
- id (BIGINT IDENTITY, PK)
- product_id (BIGINT, FK -> downloaded_product.id)
- current_price (DECIMAL(18,2))
- basic_price (DECIMAL(18,2))
- stock_status (NVARCHAR)
- Indexy:
  - INDEX(product_id)

### downloaded_variant_option
- id (BIGINT IDENTITY, PK)
- variant_id (BIGINT, FK -> downloaded_variant.id)
- option_name (NVARCHAR, NOT NULL)
- option_value (NVARCHAR, NOT NULL)
- UNIQUE(variant_id, option_name, option_value)

### autopoll_run
- id (BIGINT IDENTITY, PK)
- rule_id (BIGINT, FK -> autopoll_rule.id)  // pokud existuje tabulka pravidel
- start_at (DATETIME2, NOT NULL)
- end_at (DATETIME2, NULL)
- status (NVARCHAR, NOT NULL)
- error_message (NVARCHAR, NULL)
- items_checked (INT, NULL)
- items_changed (INT, NULL)
- duration_ms (BIGINT, NULL)
- Indexy:
  - INDEX(rule_id, start_at DESC)
  - INDEX(status)

### autopoll_snapshot (mezisklad pro batch commit)
- id (BIGINT IDENTITY, PK)
- url (NVARCHAR, NOT NULL)
- url_domain (NVARCHAR, NOT NULL)
- captured_at (DATETIME2, NOT NULL)
- source_run_id (BIGINT, FK -> autopoll_run.id)
- change_hash (NVARCHAR, NOT NULL)
- snapshot_columns: plna kopie Output.csv sloupcu (a pripadne url/is_active)
- Indexy:
  - INDEX(url_domain, captured_at DESC)
  - INDEX(change_hash)

### autopoll_change_log
- id (BIGINT IDENTITY, PK)
- snapshot_id (BIGINT, FK -> autopoll_snapshot.id)
- changed_fields (NVARCHAR)  // seznam sloupcu
- old_values (NVARCHAR(MAX), NULL)
- new_values (NVARCHAR(MAX), NULL)
- Indexy:
  - INDEX(snapshot_id)

### autopoll_batch_commit
- id (BIGINT IDENTITY, PK)
- start_at (DATETIME2, NOT NULL)
- end_at (DATETIME2, NULL)
- status (NVARCHAR, NOT NULL)
- items_applied (INT, NULL)
- report_path (NVARCHAR, NULL)
- notify_sent (BIT, NOT NULL, default 0)
- Indexy:
  - INDEX(start_at DESC)

## Autopoll reporty
- Denni souborovy report (CSV+XLS) z autopoll_snapshot (stejne jako scraper).
- Notifikace po vytvoreni reportu (konfigurovatelne).

## Config schema (navrh)

### config_eshop
- id (INT IDENTITY, PK)
- name (NVARCHAR, NOT NULL)
- base_url (NVARCHAR, NOT NULL)
- is_enabled (BIT, NOT NULL)
- download_schedule (NVARCHAR, NOT NULL)  // interval (napr. kazde X hodin)
- price_list_source (NVARCHAR, NOT NULL)  // zdroj ceniku (upload)
- language_flags (NVARCHAR, NULL)  // volitelne
- Pozn: ItemFilter se paruje pres domenu, bez reference.
- Pozn: Autopoll ma vlastni konfiguraci 1:1 s pravidly, neni v config_eshop.

### config_autopoll_rule
- id (INT IDENTITY, PK)
- name (NVARCHAR, NOT NULL)
- is_enabled (BIT, NOT NULL)
- eshop_id (INT, FK -> config_eshop.id)
- interval_value (INT, NOT NULL)  // napr. 1
- interval_unit (NVARCHAR, NOT NULL)  // minute/hour/day
- window_start (TIME, NULL)  // napr. 06:00
- window_end (TIME, NULL)    // napr. 24:00
- filter_definition (NVARCHAR(MAX), NOT NULL)  // excel-like filtr per sloupec
- last_run_at (DATETIME2, NULL)
- next_run_at (DATETIME2, NULL)
- note (NVARCHAR, NULL)
- Constraint: window_start a window_end jsou oba NULL nebo oba NOT NULL
- Pozn: vsechna pravidla bezi paralelne (bez priority, bez max_parallel)

### config_settings (key/value)
- id (INT IDENTITY, PK)
- key (NVARCHAR, UNIQUE)
- value (NVARCHAR(MAX))
- category (NVARCHAR, NULL)  // např. paths/ai/notify/other

#### Navrh klicu (min)
- paths.output_root
- paths.raw_root
- paths.images_root
- paths.reports_root
- paths.logs_root
- paths.autopoll_reports_root
- paths.temp_root
- runtime.export_format_default  // csv/xls/xlsx
- runtime.jzshop_export_enabled  // bool
- runtime.output_csv_enabled     // bool
- runtime.output_xls_enabled     // bool
- runtime.output_xlsx_enabled    // bool
- runtime.ai_enabled             // bool (override)
- runtime.autopoll_enabled       // bool (override)
- runtime.webscrape_enabled      // bool (override)
- runtime.unifier_enabled        // bool (override)
- notify.enabled_global          // bool
- notify.channels                // seznam (in-app/push)
- notify.events                  // seznam povolenych eventu

## Audit schema (navrh)

### audit_log
- id (BIGINT IDENTITY, PK)
- event_at (DATETIME2, NOT NULL, default GETUTCDATE)
- actor_type (NVARCHAR, NOT NULL)  // user/system/service
- actor_id (NVARCHAR, NULL)
- action (NVARCHAR, NOT NULL)  // create/update/delete/run/approve/...
- entity_type (NVARCHAR, NOT NULL)
- entity_id (NVARCHAR, NULL)
- summary (NVARCHAR, NULL)
- details_json (NVARCHAR(MAX), NULL)
- source_service (NVARCHAR, NULL)
- correlation_id (NVARCHAR, NULL)
- Indexy:
  - INDEX(event_at DESC)
  - INDEX(entity_type, entity_id)

## AI usage log

### ai_usage_log
- id (BIGINT IDENTITY, PK)
- event_at (DATETIME2, NOT NULL, default GETUTCDATE)
- provider_id (INT, FK -> config_ai_provider_catalog.id)
- model_name (NVARCHAR, NOT NULL)
- task_name (NVARCHAR, NOT NULL)
- request_id (NVARCHAR, NULL)
- tokens_in (INT, NULL)
- tokens_out (INT, NULL)
- cost_usd (DECIMAL(18,6), NOT NULL)
- currency (NVARCHAR, NOT NULL, default "USD")
- price_source (NVARCHAR, NOT NULL)  // aktualni a overena cena
- correlation_id (NVARCHAR, NULL)
- Indexy:
  - INDEX(event_at DESC)
  - INDEX(provider_id, model_name)

## Runtime/Lock schema (navrh)

### runtime_lock
- id (BIGINT IDENTITY, PK)
- resource_key (NVARCHAR, UNIQUE)  // napr. "webscrape:eshop_id:5"
- owner_id (NVARCHAR, NOT NULL)    // instance/service id
- started_at (DATETIME2, NOT NULL)
- last_heartbeat (DATETIME2, NOT NULL)
- expires_at (DATETIME2, NOT NULL)
- status (NVARCHAR, NOT NULL)      // running/released/stale
- Indexy:
  - UNIQUE(resource_key)
  - INDEX(expires_at)

### runtime_state
- id (BIGINT IDENTITY, PK)
- service_name (NVARCHAR, NOT NULL)
- eshop_id (INT, NULL)
- state (NVARCHAR, NOT NULL)  // idle/running/waiting/error
- updated_at (DATETIME2, NOT NULL)
- last_message (NVARCHAR, NULL)
- Indexy:
  - INDEX(service_name, updated_at DESC)
  - INDEX(eshop_id)

### Poznamky k runtime
- Lock se ziskava/release aplikaci; heartbeat se obnovuje periodicky.
- Lock je platny pouze pokud last_heartbeat je "cerstvy".
- Stale lock lze prevzit po expiraci (expires_at).

### config_languages
- id (INT IDENTITY, PK)
- code (NVARCHAR, UNIQUE)
- name (NVARCHAR)
- is_enabled (BIT, NOT NULL)
- is_default (BIT, NOT NULL)

### config_ai_provider_catalog
- id (INT IDENTITY, PK)
- name (NVARCHAR, UNIQUE)
- supports_batch (BIT)
- supports_images (BIT)
- supports_text (BIT)
- is_active (BIT)

### config_ai_provider_assignment
- id (INT IDENTITY, PK)
- task_name (NVARCHAR, NOT NULL)  // napr. standardizace, extrakce, match, ...
- provider_id (INT, FK -> config_ai_provider_catalog.id)
- model_name (NVARCHAR, NOT NULL)
- api_key_encrypted (NVARCHAR(MAX), NOT NULL)
- is_enabled (BIT, NOT NULL)

### config_ai_pricing
- id (INT IDENTITY, PK)
- provider_id (INT, FK -> config_ai_provider_catalog.id)
- model_name (NVARCHAR, NOT NULL)
- price_input_per_1k (DECIMAL(18,6), NOT NULL)
- price_output_per_1k (DECIMAL(18,6), NOT NULL)
- currency (NVARCHAR, NOT NULL, default "USD")
- source_url (NVARCHAR, NULL)
- last_updated_at (DATETIME2, NULL)

### config_pincesobchod_api_keys
- id (INT IDENTITY, PK)
- language_code (NVARCHAR, NOT NULL)
- api_key_encrypted (NVARCHAR(MAX), NOT NULL)
- is_enabled (BIT, NOT NULL)

## Memory schema (navrh, tabulky po typech)

### Memory tabulky (spolecne sloupce)
- id (INT IDENTITY, PK)
- key (NVARCHAR, NOT NULL)
- value (NVARCHAR(MAX), NOT NULL)
- language_code (NVARCHAR, NULL)  // pouze u jazykove zavislych
- source (NVARCHAR, NOT NULL)  // import/manual/ai/other
- created_at (DATETIME2, NOT NULL, default GETUTCDATE)
- updated_at (DATETIME2, NOT NULL, default GETUTCDATE)

### Jazykove nezavisle tabulky
- memory_brand_code_list
- memory_category_code_list
- memory_category_sub_code_list
- memory_category_id_list
- memory_category_list
- memory_item_filter  // paruje se na eshop pres domenu (url_domain sloupec)
- memory_wrongs  // manualni seznam vylouceni jednotlivych produktu

### Jazykove zavisle tabulky (language_code povinny)
- memory_category_mapping_glami
- memory_category_mapping_google
- memory_category_mapping_heureka
- memory_category_mapping_zbozi
- memory_category_memory
- memory_category_name_memory
- memory_desc_memory
- memory_keywords_google
- memory_keywords_zbozi
- memory_name_memory
- memory_product_brand_memory
- memory_product_model_memory
- memory_product_type_memory
- memory_short_desc_memory
- memory_variant_name_memory
- memory_variant_value_memory
- memory_stock_status_memory

### TBD
- existing_product_variants  // pouzito v parseru, rozhodnout nahradu z product_variant

### Poznamky
- DefaultExportProductValues zustavaji v kodu (ne DB).
- Import/export memory bude rozsiren o source + id + created_at + updated_at (ne 1:1 s CSV).
