-- Desaka initial schema (database-first). SQL Server flavor.
-- NOTE: Output.csv columns are stored as NVARCHAR(MAX) unless specified otherwise.

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'data') EXEC('CREATE SCHEMA data');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'scrape') EXEC('CREATE SCHEMA scrape');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'autopoll') EXEC('CREATE SCHEMA autopoll');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'memory') EXEC('CREATE SCHEMA memory');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'config') EXEC('CREATE SCHEMA config');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'notify') EXEC('CREATE SCHEMA notify');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'runtime') EXEC('CREATE SCHEMA runtime');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'audit') EXEC('CREATE SCHEMA audit');

-- data.products_current
IF OBJECT_ID('data.products_current', 'U') IS NULL
BEGIN
    CREATE TABLE data.products_current (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        kod NVARCHAR(11) NOT NULL UNIQUE,
        url NVARCHAR(2048) NOT NULL,
        url_domain NVARCHAR(512) NOT NULL,
        is_active BIT NOT NULL CONSTRAINT DF_products_current_is_active DEFAULT (1),
        created_at DATETIME2 NOT NULL CONSTRAINT DF_products_current_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_products_current_updated_at DEFAULT (SYSUTCDATETIME()),
        -- Output.csv columns
        id_output NVARCHAR(MAX) NULL,
        typ NVARCHAR(MAX) NULL,
        varianta_id NVARCHAR(MAX) NULL,
        varianta1_nazev NVARCHAR(MAX) NULL,
        varianta1_hodnota NVARCHAR(MAX) NULL,
        varianta2_nazev NVARCHAR(MAX) NULL,
        varianta2_hodnota NVARCHAR(MAX) NULL,
        varianta3_nazev NVARCHAR(MAX) NULL,
        varianta3_hodnota NVARCHAR(MAX) NULL,
        varianta_stejne NVARCHAR(MAX) NULL,
        zobrazit BIT NULL,
        archiv BIT NULL,
        kod_vyrobku NVARCHAR(MAX) NULL,
        ean NVARCHAR(MAX) NULL,
        isbn NVARCHAR(MAX) NULL,
        nazev NVARCHAR(MAX) NULL,
        privlastek NVARCHAR(MAX) NULL,
        vyrobce NVARCHAR(MAX) NULL,
        dodavatel_id NVARCHAR(MAX) NULL,
        cena DECIMAL(18,2) NULL,
        cena_bezna DECIMAL(18,2) NULL,
        cena_nakupni DECIMAL(18,2) NULL,
        recyklacni_poplatek DECIMAL(18,2) NULL,
        dph DECIMAL(5,2) NULL,
        sleva DECIMAL(5,2) NULL,
        sleva_od DATE NULL,
        sleva_do DATE NULL,
        popis NVARCHAR(MAX) NULL,
        popis_strucny NVARCHAR(MAX) NULL,
        kosik BIT NULL,
        home BIT NULL,
        dostupnost INT NULL,
        doprava_zdarma BIT NULL,
        dodaci_doba INT NULL,
        dodaci_doba_auto BIT NULL,
        sklad INT NULL,
        na_sklade INT NULL,
        hmotnost DECIMAL(18,3) NULL,
        delka DECIMAL(18,3) NULL,
        jednotka NVARCHAR(MAX) NULL,
        odber_po INT NULL,
        odber_min INT NULL,
        odber_max INT NULL,
        pocet INT NULL,
        zaruka INT NULL,
        marze_dodavatel DECIMAL(5,2) NULL,
        seo_titulek NVARCHAR(MAX) NULL,
        seo_popis NVARCHAR(MAX) NULL,
        eroticke BIT NULL,
        pro_dospele BIT NULL,
        slevovy_kupon BIT NULL,
        darek_objednavka BIT NULL,
        priorita INT NULL,
        poznamka NVARCHAR(MAX) NULL,
        dodavatel_kod NVARCHAR(MAX) NULL,
        stitky NVARCHAR(MAX) NULL,
        cena_dodavatel DECIMAL(18,2) NULL,
        kategorie_id NVARCHAR(MAX) NULL,
        podobne NVARCHAR(MAX) NULL,
        prislusenstvi NVARCHAR(MAX) NULL,
        variantove NVARCHAR(MAX) NULL,
        zdarma BIT NULL,
        sluzby NVARCHAR(MAX) NULL,
        rozsirujici_obsah NVARCHAR(MAX) NULL,
        zbozicz_skryt BIT NULL,
        zbozicz_productname NVARCHAR(MAX) NULL,
        zbozicz_product NVARCHAR(MAX) NULL,
        zbozicz_cpc DECIMAL(18,4) NULL,
        zbozicz_cpc_search DECIMAL(18,4) NULL,
        zbozicz_kategorie NVARCHAR(MAX) NULL,
        zbozicz_stitek_0 NVARCHAR(MAX) NULL,
        zbozicz_stitek_1 NVARCHAR(MAX) NULL,
        zbozicz_extra NVARCHAR(MAX) NULL,
        heurekacz_skryt BIT NULL,
        heurekacz_productname NVARCHAR(MAX) NULL,
        heurekacz_product NVARCHAR(MAX) NULL,
        heurekacz_cpc DECIMAL(18,4) NULL,
        heurekacz_kategorie NVARCHAR(MAX) NULL,
        google_skryt BIT NULL,
        google_kategorie NVARCHAR(MAX) NULL,
        google_stitek_0 NVARCHAR(MAX) NULL,
        google_stitek_1 NVARCHAR(MAX) NULL,
        google_stitek_2 NVARCHAR(MAX) NULL,
        google_stitek_3 NVARCHAR(MAX) NULL,
        google_stitek_4 NVARCHAR(MAX) NULL,
        glami_skryt BIT NULL,
        glami_kategorie NVARCHAR(MAX) NULL,
        glami_cpc DECIMAL(18,4) NULL,
        glami_voucher BIT NULL,
        glami_material NVARCHAR(MAX) NULL,
        glamisk_material NVARCHAR(MAX) NULL,
        sklad_umisteni NVARCHAR(MAX) NULL,
        sklad_minimalni INT NULL,
        sklad_optimalni INT NULL,
        sklad_maximalni INT NULL
    );

    CREATE INDEX IX_products_current_url_domain ON data.products_current (url_domain);
    CREATE INDEX IX_products_current_url ON data.products_current (url);
    CREATE INDEX IX_products_current_updated_at ON data.products_current (updated_at);
END

-- data.products_history (SCD2)
IF OBJECT_ID('data.products_history', 'U') IS NULL
BEGIN
    CREATE TABLE data.products_history (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        product_id BIGINT NOT NULL,
        valid_from DATETIME2 NOT NULL,
        valid_to DATETIME2 NULL,
        change_source NVARCHAR(64) NOT NULL,
        -- snapshot: mirror of products_current
        kod NVARCHAR(11) NOT NULL,
        url NVARCHAR(2048) NOT NULL,
        url_domain NVARCHAR(512) NOT NULL,
        is_active BIT NOT NULL,
        created_at DATETIME2 NOT NULL,
        updated_at DATETIME2 NOT NULL,
        id_output NVARCHAR(MAX) NULL,
        typ NVARCHAR(MAX) NULL,
        varianta_id NVARCHAR(MAX) NULL,
        varianta1_nazev NVARCHAR(MAX) NULL,
        varianta1_hodnota NVARCHAR(MAX) NULL,
        varianta2_nazev NVARCHAR(MAX) NULL,
        varianta2_hodnota NVARCHAR(MAX) NULL,
        varianta3_nazev NVARCHAR(MAX) NULL,
        varianta3_hodnota NVARCHAR(MAX) NULL,
        varianta_stejne NVARCHAR(MAX) NULL,
        zobrazit BIT NULL,
        archiv BIT NULL,
        kod_vyrobku NVARCHAR(MAX) NULL,
        ean NVARCHAR(MAX) NULL,
        isbn NVARCHAR(MAX) NULL,
        nazev NVARCHAR(MAX) NULL,
        privlastek NVARCHAR(MAX) NULL,
        vyrobce NVARCHAR(MAX) NULL,
        dodavatel_id NVARCHAR(MAX) NULL,
        cena DECIMAL(18,2) NULL,
        cena_bezna DECIMAL(18,2) NULL,
        cena_nakupni DECIMAL(18,2) NULL,
        recyklacni_poplatek DECIMAL(18,2) NULL,
        dph DECIMAL(5,2) NULL,
        sleva DECIMAL(5,2) NULL,
        sleva_od DATE NULL,
        sleva_do DATE NULL,
        popis NVARCHAR(MAX) NULL,
        popis_strucny NVARCHAR(MAX) NULL,
        kosik BIT NULL,
        home BIT NULL,
        dostupnost INT NULL,
        doprava_zdarma BIT NULL,
        dodaci_doba INT NULL,
        dodaci_doba_auto BIT NULL,
        sklad INT NULL,
        na_sklade INT NULL,
        hmotnost DECIMAL(18,3) NULL,
        delka DECIMAL(18,3) NULL,
        jednotka NVARCHAR(MAX) NULL,
        odber_po INT NULL,
        odber_min INT NULL,
        odber_max INT NULL,
        pocet INT NULL,
        zaruka INT NULL,
        marze_dodavatel DECIMAL(5,2) NULL,
        seo_titulek NVARCHAR(MAX) NULL,
        seo_popis NVARCHAR(MAX) NULL,
        eroticke BIT NULL,
        pro_dospele BIT NULL,
        slevovy_kupon BIT NULL,
        darek_objednavka BIT NULL,
        priorita INT NULL,
        poznamka NVARCHAR(MAX) NULL,
        dodavatel_kod NVARCHAR(MAX) NULL,
        stitky NVARCHAR(MAX) NULL,
        cena_dodavatel DECIMAL(18,2) NULL,
        kategorie_id NVARCHAR(MAX) NULL,
        podobne NVARCHAR(MAX) NULL,
        prislusenstvi NVARCHAR(MAX) NULL,
        variantove NVARCHAR(MAX) NULL,
        zdarma BIT NULL,
        sluzby NVARCHAR(MAX) NULL,
        rozsirujici_obsah NVARCHAR(MAX) NULL,
        zbozicz_skryt BIT NULL,
        zbozicz_productname NVARCHAR(MAX) NULL,
        zbozicz_product NVARCHAR(MAX) NULL,
        zbozicz_cpc DECIMAL(18,4) NULL,
        zbozicz_cpc_search DECIMAL(18,4) NULL,
        zbozicz_kategorie NVARCHAR(MAX) NULL,
        zbozicz_stitek_0 NVARCHAR(MAX) NULL,
        zbozicz_stitek_1 NVARCHAR(MAX) NULL,
        zbozicz_extra NVARCHAR(MAX) NULL,
        heurekacz_skryt BIT NULL,
        heurekacz_productname NVARCHAR(MAX) NULL,
        heurekacz_product NVARCHAR(MAX) NULL,
        heurekacz_cpc DECIMAL(18,4) NULL,
        heurekacz_kategorie NVARCHAR(MAX) NULL,
        google_skryt BIT NULL,
        google_kategorie NVARCHAR(MAX) NULL,
        google_stitek_0 NVARCHAR(MAX) NULL,
        google_stitek_1 NVARCHAR(MAX) NULL,
        google_stitek_2 NVARCHAR(MAX) NULL,
        google_stitek_3 NVARCHAR(MAX) NULL,
        google_stitek_4 NVARCHAR(MAX) NULL,
        glami_skryt BIT NULL,
        glami_kategorie NVARCHAR(MAX) NULL,
        glami_cpc DECIMAL(18,4) NULL,
        glami_voucher BIT NULL,
        glami_material NVARCHAR(MAX) NULL,
        glamisk_material NVARCHAR(MAX) NULL,
        sklad_umisteni NVARCHAR(MAX) NULL,
        sklad_minimalni INT NULL,
        sklad_optimalni INT NULL,
        sklad_maximalni INT NULL
    );

    CREATE INDEX IX_products_history_product_id_valid_from ON data.products_history (product_id, valid_from DESC);
    CREATE INDEX IX_products_history_valid_to ON data.products_history (valid_to);
END

-- data.product_variant
IF OBJECT_ID('data.product_variant', 'U') IS NULL
BEGIN
    CREATE TABLE data.product_variant (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        product_id BIGINT NOT NULL,
        variantcode NVARCHAR(14) NOT NULL UNIQUE,
        current_price DECIMAL(18,2) NULL,
        basic_price DECIMAL(18,2) NULL,
        stock_status NVARCHAR(128) NULL
    );
    CREATE INDEX IX_product_variant_product_id ON data.product_variant (product_id);
END

-- data.variant_option
IF OBJECT_ID('data.variant_option', 'U') IS NULL
BEGIN
    CREATE TABLE data.variant_option (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        variant_id BIGINT NOT NULL,
        option_name NVARCHAR(256) NOT NULL,
        option_value NVARCHAR(256) NOT NULL,
        CONSTRAINT UQ_variant_option UNIQUE (variant_id, option_name, option_value)
    );
END

-- scrape.run
IF OBJECT_ID('scrape.scrape_run', 'U') IS NULL
BEGIN
    CREATE TABLE scrape.scrape_run (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        eshop_id INT NOT NULL,
        start_at DATETIME2 NOT NULL,
        end_at DATETIME2 NULL,
        status NVARCHAR(32) NOT NULL,
        error_message NVARCHAR(MAX) NULL,
        items_total INT NULL,
        items_changed INT NULL,
        items_skipped INT NULL,
        duration_ms BIGINT NULL
    );
    CREATE INDEX IX_scrape_run_eshop_id_start_at ON scrape.scrape_run (eshop_id, start_at DESC);
    CREATE INDEX IX_scrape_run_status ON scrape.scrape_run (status);
END

-- scrape.raw metadata
IF OBJECT_ID('scrape.scrape_raw_metadata', 'U') IS NULL
BEGIN
    CREATE TABLE scrape.scrape_raw_metadata (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        eshop_id INT NOT NULL,
        url NVARCHAR(2048) NOT NULL,
        file_path NVARCHAR(1024) NOT NULL,
        content_hash NVARCHAR(128) NOT NULL,
        content_size BIGINT NULL,
        content_type NVARCHAR(16) NOT NULL,
        downloaded_at DATETIME2 NOT NULL
    );
    CREATE INDEX IX_scrape_raw_eshop_id_downloaded_at ON scrape.scrape_raw_metadata (eshop_id, downloaded_at DESC);
    CREATE INDEX IX_scrape_raw_content_hash ON scrape.scrape_raw_metadata (content_hash);
END

-- scrape.downloaded_product
IF OBJECT_ID('scrape.downloaded_product', 'U') IS NULL
BEGIN
    CREATE TABLE scrape.downloaded_product (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        eshop_id INT NOT NULL,
        name NVARCHAR(512) NOT NULL,
        short_description NVARCHAR(MAX) NULL,
        description NVARCHAR(MAX) NULL,
        main_photo_path NVARCHAR(1024) NULL,
        url NVARCHAR(2048) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_downloaded_product_created_at DEFAULT (SYSUTCDATETIME())
    );
    CREATE INDEX IX_downloaded_product_eshop_id_created_at ON scrape.downloaded_product (eshop_id, created_at DESC);
    CREATE INDEX IX_downloaded_product_url ON scrape.downloaded_product (url);
END

IF OBJECT_ID('scrape.downloaded_gallery', 'U') IS NULL
BEGIN
    CREATE TABLE scrape.downloaded_gallery (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        product_id BIGINT NOT NULL,
        filepath NVARCHAR(1024) NOT NULL
    );
    CREATE INDEX IX_downloaded_gallery_product_id ON scrape.downloaded_gallery (product_id);
END

IF OBJECT_ID('scrape.downloaded_variant', 'U') IS NULL
BEGIN
    CREATE TABLE scrape.downloaded_variant (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        product_id BIGINT NOT NULL,
        current_price DECIMAL(18,2) NULL,
        basic_price DECIMAL(18,2) NULL,
        stock_status NVARCHAR(128) NULL
    );
    CREATE INDEX IX_downloaded_variant_product_id ON scrape.downloaded_variant (product_id);
END

IF OBJECT_ID('scrape.downloaded_variant_option', 'U') IS NULL
BEGIN
    CREATE TABLE scrape.downloaded_variant_option (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        variant_id BIGINT NOT NULL,
        option_name NVARCHAR(256) NOT NULL,
        option_value NVARCHAR(256) NOT NULL,
        CONSTRAINT UQ_downloaded_variant_option UNIQUE (variant_id, option_name, option_value)
    );
END

-- autopoll tables
IF OBJECT_ID('autopoll.autopoll_run', 'U') IS NULL
BEGIN
    CREATE TABLE autopoll.autopoll_run (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        rule_id BIGINT NULL,
        start_at DATETIME2 NOT NULL,
        end_at DATETIME2 NULL,
        status NVARCHAR(32) NOT NULL,
        error_message NVARCHAR(MAX) NULL,
        items_checked INT NULL,
        items_changed INT NULL,
        duration_ms BIGINT NULL
    );
    CREATE INDEX IX_autopoll_run_rule_id_start_at ON autopoll.autopoll_run (rule_id, start_at DESC);
    CREATE INDEX IX_autopoll_run_status ON autopoll.autopoll_run (status);
END

IF OBJECT_ID('autopoll.autopoll_snapshot', 'U') IS NULL
BEGIN
    CREATE TABLE autopoll.autopoll_snapshot (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        url NVARCHAR(2048) NOT NULL,
        url_domain NVARCHAR(512) NOT NULL,
        captured_at DATETIME2 NOT NULL,
        source_run_id BIGINT NOT NULL,
        change_hash NVARCHAR(128) NOT NULL,
        kod NVARCHAR(11) NULL,
        id_output NVARCHAR(MAX) NULL,
        typ NVARCHAR(MAX) NULL,
        varianta_id NVARCHAR(MAX) NULL,
        varianta1_nazev NVARCHAR(MAX) NULL,
        varianta1_hodnota NVARCHAR(MAX) NULL,
        varianta2_nazev NVARCHAR(MAX) NULL,
        varianta2_hodnota NVARCHAR(MAX) NULL,
        varianta3_nazev NVARCHAR(MAX) NULL,
        varianta3_hodnota NVARCHAR(MAX) NULL,
        varianta_stejne NVARCHAR(MAX) NULL,
        zobrazit BIT NULL,
        archiv BIT NULL,
        kod_vyrobku NVARCHAR(MAX) NULL,
        ean NVARCHAR(MAX) NULL,
        isbn NVARCHAR(MAX) NULL,
        nazev NVARCHAR(MAX) NULL,
        privlastek NVARCHAR(MAX) NULL,
        vyrobce NVARCHAR(MAX) NULL,
        dodavatel_id NVARCHAR(MAX) NULL,
        cena DECIMAL(18,2) NULL,
        cena_bezna DECIMAL(18,2) NULL,
        cena_nakupni DECIMAL(18,2) NULL,
        recyklacni_poplatek DECIMAL(18,2) NULL,
        dph DECIMAL(5,2) NULL,
        sleva DECIMAL(5,2) NULL,
        sleva_od DATE NULL,
        sleva_do DATE NULL,
        popis NVARCHAR(MAX) NULL,
        popis_strucny NVARCHAR(MAX) NULL,
        kosik BIT NULL,
        home BIT NULL,
        dostupnost INT NULL,
        doprava_zdarma BIT NULL,
        dodaci_doba INT NULL,
        dodaci_doba_auto BIT NULL,
        sklad INT NULL,
        na_sklade INT NULL,
        hmotnost DECIMAL(18,3) NULL,
        delka DECIMAL(18,3) NULL,
        jednotka NVARCHAR(MAX) NULL,
        odber_po INT NULL,
        odber_min INT NULL,
        odber_max INT NULL,
        pocet INT NULL,
        zaruka INT NULL,
        marze_dodavatel DECIMAL(5,2) NULL,
        seo_titulek NVARCHAR(MAX) NULL,
        seo_popis NVARCHAR(MAX) NULL,
        eroticke BIT NULL,
        pro_dospele BIT NULL,
        slevovy_kupon BIT NULL,
        darek_objednavka BIT NULL,
        priorita INT NULL,
        poznamka NVARCHAR(MAX) NULL,
        dodavatel_kod NVARCHAR(MAX) NULL,
        stitky NVARCHAR(MAX) NULL,
        cena_dodavatel DECIMAL(18,2) NULL,
        kategorie_id NVARCHAR(MAX) NULL,
        podobne NVARCHAR(MAX) NULL,
        prislusenstvi NVARCHAR(MAX) NULL,
        variantove NVARCHAR(MAX) NULL,
        zdarma BIT NULL,
        sluzby NVARCHAR(MAX) NULL,
        rozsirujici_obsah NVARCHAR(MAX) NULL,
        zbozicz_skryt BIT NULL,
        zbozicz_productname NVARCHAR(MAX) NULL,
        zbozicz_product NVARCHAR(MAX) NULL,
        zbozicz_cpc DECIMAL(18,4) NULL,
        zbozicz_cpc_search DECIMAL(18,4) NULL,
        zbozicz_kategorie NVARCHAR(MAX) NULL,
        zbozicz_stitek_0 NVARCHAR(MAX) NULL,
        zbozicz_stitek_1 NVARCHAR(MAX) NULL,
        zbozicz_extra NVARCHAR(MAX) NULL,
        heurekacz_skryt BIT NULL,
        heurekacz_productname NVARCHAR(MAX) NULL,
        heurekacz_product NVARCHAR(MAX) NULL,
        heurekacz_cpc DECIMAL(18,4) NULL,
        heurekacz_kategorie NVARCHAR(MAX) NULL,
        google_skryt BIT NULL,
        google_kategorie NVARCHAR(MAX) NULL,
        google_stitek_0 NVARCHAR(MAX) NULL,
        google_stitek_1 NVARCHAR(MAX) NULL,
        google_stitek_2 NVARCHAR(MAX) NULL,
        google_stitek_3 NVARCHAR(MAX) NULL,
        google_stitek_4 NVARCHAR(MAX) NULL,
        glami_skryt BIT NULL,
        glami_kategorie NVARCHAR(MAX) NULL,
        glami_cpc DECIMAL(18,4) NULL,
        glami_voucher BIT NULL,
        glami_material NVARCHAR(MAX) NULL,
        glamisk_material NVARCHAR(MAX) NULL,
        sklad_umisteni NVARCHAR(MAX) NULL,
        sklad_minimalni INT NULL,
        sklad_optimalni INT NULL,
        sklad_maximalni INT NULL
    );
    CREATE INDEX IX_autopoll_snapshot_url_domain_captured_at ON autopoll.autopoll_snapshot (url_domain, captured_at DESC);
    CREATE INDEX IX_autopoll_snapshot_change_hash ON autopoll.autopoll_snapshot (change_hash);
END

IF OBJECT_ID('autopoll.autopoll_change_log', 'U') IS NULL
BEGIN
    CREATE TABLE autopoll.autopoll_change_log (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        snapshot_id BIGINT NOT NULL,
        changed_fields NVARCHAR(MAX) NULL,
        old_values NVARCHAR(MAX) NULL,
        new_values NVARCHAR(MAX) NULL
    );
    CREATE INDEX IX_autopoll_change_log_snapshot_id ON autopoll.autopoll_change_log (snapshot_id);
END

IF OBJECT_ID('autopoll.autopoll_batch_commit', 'U') IS NULL
BEGIN
    CREATE TABLE autopoll.autopoll_batch_commit (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        start_at DATETIME2 NOT NULL,
        end_at DATETIME2 NULL,
        status NVARCHAR(32) NOT NULL,
        items_applied INT NULL,
        report_path NVARCHAR(1024) NULL,
        notify_sent BIT NOT NULL CONSTRAINT DF_autopoll_batch_commit_notify_sent DEFAULT (0)
    );
    CREATE INDEX IX_autopoll_batch_commit_start_at ON autopoll.autopoll_batch_commit (start_at DESC);
END

-- unifier run + approvals
IF OBJECT_ID('data.unifier_run', 'U') IS NULL
BEGIN
    CREATE TABLE data.unifier_run (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        start_at DATETIME2 NOT NULL,
        end_at DATETIME2 NULL,
        status NVARCHAR(32) NOT NULL,
        error_message NVARCHAR(MAX) NULL,
        items_total INT NULL,
        items_processed INT NULL
    );
    CREATE INDEX IX_unifier_run_start_at ON data.unifier_run (start_at DESC);
END

IF OBJECT_ID('data.unifier_approval', 'U') IS NULL
BEGIN
    CREATE TABLE data.unifier_approval (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        product_code NVARCHAR(64) NOT NULL,
        url NVARCHAR(2048) NOT NULL,
        image_url NVARCHAR(2048) NULL,
        property_name NVARCHAR(128) NOT NULL,
        current_value NVARCHAR(MAX) NULL,
        suggested_value NVARCHAR(MAX) NULL,
        language NVARCHAR(16) NOT NULL,
        status NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_unifier_approval_created_at DEFAULT (SYSUTCDATETIME()),
        resolved_at DATETIME2 NULL
    );
    CREATE INDEX IX_unifier_approval_status ON data.unifier_approval (status);
END

-- config tables
IF OBJECT_ID('config.config_eshop', 'U') IS NULL
BEGIN
    CREATE TABLE config.config_eshop (
        id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(256) NOT NULL,
        base_url NVARCHAR(2048) NOT NULL,
        is_enabled BIT NOT NULL,
        download_schedule NVARCHAR(128) NOT NULL,
        price_list_source NVARCHAR(512) NOT NULL,
        language_flags NVARCHAR(256) NULL
    );
END

IF OBJECT_ID('config.config_autopoll_rule', 'U') IS NULL
BEGIN
    CREATE TABLE config.config_autopoll_rule (
        id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(256) NOT NULL,
        is_enabled BIT NOT NULL,
        eshop_id INT NOT NULL,
        interval_value INT NOT NULL,
        interval_unit NVARCHAR(16) NOT NULL,
        window_start TIME NULL,
        window_end TIME NULL,
        filter_definition NVARCHAR(MAX) NOT NULL,
        last_run_at DATETIME2 NULL,
        next_run_at DATETIME2 NULL,
        note NVARCHAR(MAX) NULL,
        CONSTRAINT CK_autopoll_rule_window CHECK (
            (window_start IS NULL AND window_end IS NULL) OR
            (window_start IS NOT NULL AND window_end IS NOT NULL)
        )
    );
END

IF OBJECT_ID('config.config_settings', 'U') IS NULL
BEGIN
    CREATE TABLE config.config_settings (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(256) NOT NULL UNIQUE,
        [value] NVARCHAR(MAX) NULL,
        category NVARCHAR(64) NULL
    );
END

IF OBJECT_ID('config.config_languages', 'U') IS NULL
BEGIN
    CREATE TABLE config.config_languages (
        id INT IDENTITY(1,1) PRIMARY KEY,
        code NVARCHAR(16) NOT NULL UNIQUE,
        name NVARCHAR(128) NULL,
        is_enabled BIT NOT NULL,
        is_default BIT NOT NULL
    );
END

IF OBJECT_ID('config.config_ai_provider_catalog', 'U') IS NULL
BEGIN
    CREATE TABLE config.config_ai_provider_catalog (
        id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(128) NOT NULL UNIQUE,
        supports_batch BIT NULL,
        supports_images BIT NULL,
        supports_text BIT NULL,
        is_active BIT NULL
    );
END

IF OBJECT_ID('config.config_ai_provider_assignment', 'U') IS NULL
BEGIN
    CREATE TABLE config.config_ai_provider_assignment (
        id INT IDENTITY(1,1) PRIMARY KEY,
        task_name NVARCHAR(128) NOT NULL,
        provider_id INT NOT NULL,
        model_name NVARCHAR(128) NOT NULL,
        api_key_encrypted NVARCHAR(MAX) NOT NULL,
        is_enabled BIT NOT NULL
    );
END

IF OBJECT_ID('config.config_ai_pricing', 'U') IS NULL
BEGIN
    CREATE TABLE config.config_ai_pricing (
        id INT IDENTITY(1,1) PRIMARY KEY,
        provider_id INT NOT NULL,
        model_name NVARCHAR(128) NOT NULL,
        price_input_per_1k DECIMAL(18,6) NOT NULL,
        price_output_per_1k DECIMAL(18,6) NOT NULL,
        currency NVARCHAR(8) NOT NULL CONSTRAINT DF_ai_pricing_currency DEFAULT ('USD'),
        source_url NVARCHAR(2048) NULL,
        last_updated_at DATETIME2 NULL
    );
END

IF OBJECT_ID('config.config_pincesobchod_api_keys', 'U') IS NULL
BEGIN
    CREATE TABLE config.config_pincesobchod_api_keys (
        id INT IDENTITY(1,1) PRIMARY KEY,
        language_code NVARCHAR(16) NOT NULL,
        api_key_encrypted NVARCHAR(MAX) NOT NULL,
        is_enabled BIT NOT NULL
    );
END

-- memory tables (language-independent)
IF OBJECT_ID('memory.memory_brand_code_list', 'U') IS NULL
    CREATE TABLE memory.memory_brand_code_list (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_brand_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_brand_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_category_code_list', 'U') IS NULL
    CREATE TABLE memory.memory_category_code_list (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_catcode_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_catcode_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_category_sub_code_list', 'U') IS NULL
    CREATE TABLE memory.memory_category_sub_code_list (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_catsubcode_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_catsubcode_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_category_id_list', 'U') IS NULL
    CREATE TABLE memory.memory_category_id_list (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_catid_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_catid_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_category_list', 'U') IS NULL
    CREATE TABLE memory.memory_category_list (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_cat_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_cat_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_item_filter', 'U') IS NULL
    CREATE TABLE memory.memory_item_filter (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        url_domain NVARCHAR(512) NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_itemfilter_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_itemfilter_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_wrongs', 'U') IS NULL
    CREATE TABLE memory.memory_wrongs (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_wrongs_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_wrongs_updated_at DEFAULT (SYSUTCDATETIME())
    );

-- memory tables (language-dependent)
IF OBJECT_ID('memory.memory_category_mapping_glami', 'U') IS NULL
    CREATE TABLE memory.memory_category_mapping_glami (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_glami_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_glami_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_category_mapping_google', 'U') IS NULL
    CREATE TABLE memory.memory_category_mapping_google (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_google_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_google_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_category_mapping_heureka', 'U') IS NULL
    CREATE TABLE memory.memory_category_mapping_heureka (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_heureka_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_heureka_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_category_mapping_zbozi', 'U') IS NULL
    CREATE TABLE memory.memory_category_mapping_zbozi (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_zbozi_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_zbozi_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_category_memory', 'U') IS NULL
    CREATE TABLE memory.memory_category_memory (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_catmem_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_catmem_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_category_name_memory', 'U') IS NULL
    CREATE TABLE memory.memory_category_name_memory (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_catname_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_catname_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_desc_memory', 'U') IS NULL
    CREATE TABLE memory.memory_desc_memory (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_desc_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_desc_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_keywords_google', 'U') IS NULL
    CREATE TABLE memory.memory_keywords_google (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_kwgoogle_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_kwgoogle_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_keywords_zbozi', 'U') IS NULL
    CREATE TABLE memory.memory_keywords_zbozi (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_kwzbozi_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_kwzbozi_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_name_memory', 'U') IS NULL
    CREATE TABLE memory.memory_name_memory (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_name_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_name_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_product_brand_memory', 'U') IS NULL
    CREATE TABLE memory.memory_product_brand_memory (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_brandmem_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_brandmem_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_product_model_memory', 'U') IS NULL
    CREATE TABLE memory.memory_product_model_memory (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_modelmem_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_modelmem_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_product_type_memory', 'U') IS NULL
    CREATE TABLE memory.memory_product_type_memory (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_typemem_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_typemem_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_short_desc_memory', 'U') IS NULL
    CREATE TABLE memory.memory_short_desc_memory (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_shortdesc_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_shortdesc_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_variant_name_memory', 'U') IS NULL
    CREATE TABLE memory.memory_variant_name_memory (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_varname_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_varname_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_variant_value_memory', 'U') IS NULL
    CREATE TABLE memory.memory_variant_value_memory (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_varvalue_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_varvalue_updated_at DEFAULT (SYSUTCDATETIME())
    );

IF OBJECT_ID('memory.memory_stock_status_memory', 'U') IS NULL
    CREATE TABLE memory.memory_stock_status_memory (
        id INT IDENTITY(1,1) PRIMARY KEY,
        [key] NVARCHAR(512) NOT NULL,
        [value] NVARCHAR(MAX) NOT NULL,
        language_code NVARCHAR(16) NOT NULL,
        source NVARCHAR(32) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_mem_stock_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_mem_stock_updated_at DEFAULT (SYSUTCDATETIME())
    );

-- audit log
IF OBJECT_ID('audit.audit_log', 'U') IS NULL
BEGIN
    CREATE TABLE audit.audit_log (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        event_at DATETIME2 NOT NULL CONSTRAINT DF_audit_log_event_at DEFAULT (SYSUTCDATETIME()),
        actor_type NVARCHAR(32) NOT NULL,
        actor_id NVARCHAR(128) NULL,
        action NVARCHAR(64) NOT NULL,
        entity_type NVARCHAR(128) NOT NULL,
        entity_id NVARCHAR(128) NULL,
        summary NVARCHAR(MAX) NULL,
        details_json NVARCHAR(MAX) NULL,
        source_service NVARCHAR(128) NULL,
        correlation_id NVARCHAR(128) NULL
    );
    CREATE INDEX IX_audit_log_event_at ON audit.audit_log (event_at DESC);
    CREATE INDEX IX_audit_log_entity ON audit.audit_log (entity_type, entity_id);
END

-- ai usage log
IF OBJECT_ID('audit.ai_usage_log', 'U') IS NULL
BEGIN
    CREATE TABLE audit.ai_usage_log (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        event_at DATETIME2 NOT NULL CONSTRAINT DF_ai_usage_log_event_at DEFAULT (SYSUTCDATETIME()),
        provider_id INT NOT NULL,
        model_name NVARCHAR(128) NOT NULL,
        task_name NVARCHAR(128) NOT NULL,
        request_id NVARCHAR(128) NULL,
        tokens_in INT NULL,
        tokens_out INT NULL,
        cost_usd DECIMAL(18,6) NOT NULL,
        currency NVARCHAR(8) NOT NULL CONSTRAINT DF_ai_usage_currency DEFAULT ('USD'),
        price_source NVARCHAR(256) NOT NULL,
        correlation_id NVARCHAR(128) NULL
    );
    CREATE INDEX IX_ai_usage_event_at ON audit.ai_usage_log (event_at DESC);
    CREATE INDEX IX_ai_usage_provider_model ON audit.ai_usage_log (provider_id, model_name);
END

-- runtime
IF OBJECT_ID('runtime.runtime_lock', 'U') IS NULL
BEGIN
    CREATE TABLE runtime.runtime_lock (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        resource_key NVARCHAR(256) NOT NULL UNIQUE,
        owner_id NVARCHAR(128) NOT NULL,
        started_at DATETIME2 NOT NULL,
        last_heartbeat DATETIME2 NOT NULL,
        expires_at DATETIME2 NOT NULL,
        status NVARCHAR(32) NOT NULL
    );
    CREATE INDEX IX_runtime_lock_expires_at ON runtime.runtime_lock (expires_at);
END

IF OBJECT_ID('runtime.runtime_state', 'U') IS NULL
BEGIN
    CREATE TABLE runtime.runtime_state (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        service_name NVARCHAR(128) NOT NULL,
        eshop_id INT NULL,
        state NVARCHAR(32) NOT NULL,
        updated_at DATETIME2 NOT NULL,
        last_message NVARCHAR(MAX) NULL
    );
    CREATE INDEX IX_runtime_state_service_updated_at ON runtime.runtime_state (service_name, updated_at DESC);
    CREATE INDEX IX_runtime_state_eshop_id ON runtime.runtime_state (eshop_id);
END

-- notify (minimal placeholder, extend later)
IF OBJECT_ID('notify.notify_device_token', 'U') IS NULL
BEGIN
    CREATE TABLE notify.notify_device_token (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        device_id NVARCHAR(256) NOT NULL,
        platform NVARCHAR(32) NOT NULL,
        token NVARCHAR(MAX) NOT NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_notify_token_created_at DEFAULT (SYSUTCDATETIME()),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_notify_token_updated_at DEFAULT (SYSUTCDATETIME())
    );
END

