namespace Desaka.Contracts.Common;

public static class JzShopColumns
{
    public static readonly IReadOnlyList<string> All = new List<string>
    {
        "id",
        "typ",
        "varianta_id",
        "varianta1_nazev",
        "varianta1_hodnota",
        "varianta2_nazev",
        "varianta2_hodnota",
        "varianta3_nazev",
        "varianta3_hodnota",
        "varianta_stejne",
        "zobrazit",
        "archiv",
        "kod",
        "kod_vyrobku",
        "ean",
        "isbn",
        "nazev",
        "privlastek",
        "vyrobce",
        "dodavatel_id",
        "cena",
        "cena_bezna",
        "cena_nakupni",
        "recyklacni_poplatek",
        "dph",
        "sleva",
        "sleva_od",
        "sleva_do",
        "popis",
        "popis_strucny",
        "kosik",
        "home",
        "dostupnost",
        "doprava_zdarma",
        "dodaci_doba",
        "dodaci_doba_auto",
        "sklad",
        "na_sklade",
        "hmotnost",
        "delka",
        "jednotka",
        "odber_po",
        "odber_min",
        "odber_max",
        "pocet",
        "zaruka",
        "marze_dodavatel",
        "seo_titulek",
        "seo_popis",
        "eroticke",
        "pro_dospele",
        "slevovy_kupon",
        "darek_objednavka",
        "priorita",
        "poznamka",
        "dodavatel_kod",
        "stitky",
        "cena_dodavatel",
        "kategorie_id",
        "podobne",
        "prislusenstvi",
        "variantove",
        "zdarma",
        "sluzby",
        "rozsirujici_obsah",
        "zbozicz_skryt",
        "zbozicz_productname",
        "zbozicz_product",
        "zbozicz_cpc",
        "zbozicz_cpc_search",
        "zbozicz_kategorie",
        "zbozicz_stitek_0",
        "zbozicz_stitek_1",
        "zbozicz_extra",
        "heurekacz_skryt",
        "heurekacz_productname",
        "heurekacz_product",
        "heurekacz_cpc",
        "heurekacz_kategorie",
        "google_skryt",
        "google_kategorie",
        "google_stitek_0",
        "google_stitek_1",
        "google_stitek_2",
        "google_stitek_3",
        "google_stitek_4",
        "glami_skryt",
        "glami_kategorie",
        "glami_cpc",
        "glami_voucher",
        "glami_material",
        "glamisk_material",
        "sklad_umisteni",
        "sklad_minimalni",
        "sklad_optimalni",
        "sklad_maximalni"
    };

    private static readonly Dictionary<string, string> ColumnOverrides = new(StringComparer.OrdinalIgnoreCase)
    {
        { "id", "IdOutput" },
        { "kod", "Kod" },
        { "pro_dospele", "ProDospele" },
        { "darek_objednavka", "DarekObjednavka" }
    };

    public static string ColumnToPropertyName(string column)
    {
        if (ColumnOverrides.TryGetValue(column, out var overrideName))
        {
            return overrideName;
        }

        if (column.StartsWith("zbozicz_", StringComparison.OrdinalIgnoreCase))
        {
            return "ZboziCz" + ToPascal(column.Substring("zbozicz_".Length));
        }

        if (column.StartsWith("heurekacz_", StringComparison.OrdinalIgnoreCase))
        {
            return "HeurekaCz" + ToPascal(column.Substring("heurekacz_".Length));
        }

        if (column.StartsWith("glamisk_", StringComparison.OrdinalIgnoreCase))
        {
            return "GlamiSk" + ToPascal(column.Substring("glamisk_".Length));
        }

        return ToPascal(column);
    }

    private static string ToPascal(string column)
    {
        var parts = column.Split('_', StringSplitOptions.RemoveEmptyEntries);
        var builder = new System.Text.StringBuilder();
        foreach (var part in parts)
        {
            if (part.Length == 0)
            {
                continue;
            }

            builder.Append(char.ToUpperInvariant(part[0]));
            if (part.Length > 1)
            {
                builder.Append(part.Substring(1));
            }
        }

        return builder.ToString();
    }
}
