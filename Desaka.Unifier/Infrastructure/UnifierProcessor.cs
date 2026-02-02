using Desaka.DataAccess;
using Desaka.DataAccess.Entities;
using Desaka.Unifying.Models;
using Microsoft.EntityFrameworkCore;
using DownloadedProductModel = Desaka.Unifying.Models.DownloadedProduct;
using DownloadedVariantModel = Desaka.Unifying.Models.DownloadedVariant;
using DownloadedVariantOptionModel = Desaka.Unifying.Models.DownloadedVariantOption;

namespace Desaka.Unifier.Infrastructure;

public sealed class UnifierProcessor
{
    private readonly DesakaDbContext _db;
    private readonly MemoryLookupService _memoryLookup;
    private readonly CodeGenerator _codeGenerator;

    public UnifierProcessor(DesakaDbContext db, MemoryLookupService memoryLookup, CodeGenerator codeGenerator)
    {
        _db = db;
        _memoryLookup = memoryLookup;
        _codeGenerator = codeGenerator;
    }

    public async Task<UnifierProcessingResult> ProcessAsync(UnifierRun run, CancellationToken cancellationToken)
    {
        var downloaded = await LoadDownloadedProductsAsync(cancellationToken);
        var approvals = new List<UnifierApproval>();
        var products = new List<ProductsCurrent>();

        foreach (var item in downloaded)
        {
            var repaired = await RepairAsync(item, approvals, cancellationToken);
            if (repaired == null)
            {
                continue;
            }

            var mapped = await MapToProductsAsync(repaired, item, cancellationToken);
            products.AddRange(mapped);
        }

        if (approvals.Count > 0)
        {
            _db.UnifierApprovals.AddRange(approvals);
            await _db.SaveChangesAsync(cancellationToken);
            return new UnifierProcessingResult(UnifierProcessOutcome.WaitingApproval, products, approvals);
        }

        await UpsertProductsAsync(products, cancellationToken);
        return new UnifierProcessingResult(UnifierProcessOutcome.Success, products, approvals);
    }

    private async Task<List<DownloadedProductModel>> LoadDownloadedProductsAsync(CancellationToken cancellationToken)
    {
        var products = await _db.DownloadedProducts.AsNoTracking().ToListAsync(cancellationToken);
        var galleries = await _db.DownloadedGalleries.AsNoTracking().ToListAsync(cancellationToken);
        var variants = await _db.DownloadedVariants.AsNoTracking().ToListAsync(cancellationToken);
        var options = await _db.DownloadedVariantOptions.AsNoTracking().ToListAsync(cancellationToken);

        var galleryLookup = galleries.GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.Select(g => g.Filepath).ToList());
        var optionLookup = options.GroupBy(x => x.VariantId).ToDictionary(x => x.Key, x => x.ToList());
        var variantLookup = variants.GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.ToList());

        var result = new List<DownloadedProductModel>();
        foreach (var product in products)
        {
            var model = new DownloadedProductModel
            {
                Id = product.Id,
                EshopId = product.EshopId,
                Name = product.Name,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                MainPhotoPath = product.MainPhotoPath,
                Url = product.Url,
                CreatedAt = product.CreatedAt
            };

            if (galleryLookup.TryGetValue(product.Id, out var gallery))
            {
                model.GalleryFilepaths.AddRange(gallery);
            }

            if (variantLookup.TryGetValue(product.Id, out var variantItems))
            {
                foreach (var variant in variantItems)
                {
                    var variantModel = new DownloadedVariantModel
                    {
                        Id = variant.Id,
                        CurrentPrice = variant.CurrentPrice,
                        BasicPrice = variant.BasicPrice,
                        StockStatus = variant.StockStatus
                    };

                    if (optionLookup.TryGetValue(variant.Id, out var variantOptions))
                    {
                        foreach (var option in variantOptions)
                        {
                            variantModel.Options.Add(new DownloadedVariantOptionModel
                            {
                                OptionName = option.OptionName,
                                OptionValue = option.OptionValue
                            });
                        }
                    }

                    model.Variants.Add(variantModel);
                }
            }

            result.Add(model);
        }

        return result;
    }

    private async Task<RepairedProduct?> RepairAsync(DownloadedProduct downloaded, List<UnifierApproval> approvals, CancellationToken cancellationToken)
    {
        var name = downloaded.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var repaired = new RepairedProduct
        {
            OriginalName = name,
            Name = await _memoryLookup.LookupExactAsync("name_memory", name, "cs", cancellationToken) ?? name,
            ShortDesc = downloaded.ShortDescription ?? await _memoryLookup.LookupExactAsync("short_desc_memory", name, "cs", cancellationToken) ?? string.Empty,
            Desc = downloaded.Description ?? await _memoryLookup.LookupExactAsync("desc_memory", name, "cs", cancellationToken) ?? string.Empty,
            Url = downloaded.Url,
            Brand = await _memoryLookup.LookupExactAsync("product_brand_memory", name, "cs", cancellationToken) ?? string.Empty,
            Type = await _memoryLookup.LookupExactAsync("product_type_memory", name, "cs", cancellationToken) ?? string.Empty,
            Model = await _memoryLookup.LookupExactAsync("product_model_memory", name, "cs", cancellationToken) ?? string.Empty,
            Category = await _memoryLookup.LookupExactAsync("category_memory", name, "cs", cancellationToken) ?? string.Empty,
            CategoryIds = string.Empty,
            GoogleCategory = string.Empty,
            HeurekaCategory = string.Empty,
            ZboziCategory = string.Empty,
            GlamiCategory = string.Empty,
            GoogleKeywords = await _memoryLookup.LookupExactAsync("keywords_google", name, "cs", cancellationToken) ?? string.Empty,
            ZboziKeywords = await _memoryLookup.LookupExactAsync("keywords_zbozi", name, "cs", cancellationToken) ?? string.Empty
        };

        var categoryKey = string.IsNullOrWhiteSpace(repaired.Category) ? name : repaired.Category;
        repaired.CategoryIds = await _memoryLookup.LookupExactAsync("category_id_list", categoryKey, null, cancellationToken) ?? string.Empty;
        repaired.GoogleCategory = await _memoryLookup.LookupExactAsync("category_mapping_google", categoryKey, "cs", cancellationToken) ?? string.Empty;
        repaired.HeurekaCategory = await _memoryLookup.LookupExactAsync("category_mapping_heureka", categoryKey, "cs", cancellationToken) ?? string.Empty;
        repaired.ZboziCategory = await _memoryLookup.LookupExactAsync("category_mapping_zbozi", categoryKey, "cs", cancellationToken) ?? string.Empty;
        repaired.GlamiCategory = await _memoryLookup.LookupExactAsync("category_mapping_glami", categoryKey, "cs", cancellationToken) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(repaired.Brand))
        {
            approvals.Add(BuildApproval(repaired, "brand", null));
        }

        if (string.IsNullOrWhiteSpace(repaired.Category))
        {
            approvals.Add(BuildApproval(repaired, "category", null));
        }

        if (string.IsNullOrWhiteSpace(repaired.CategoryIds))
        {
            approvals.Add(BuildApproval(repaired, "category_ids", null));
        }

        if (string.IsNullOrWhiteSpace(repaired.Name))
        {
            approvals.Add(BuildApproval(repaired, "name", null));
        }

        return repaired;
    }

    private async Task<List<ProductsCurrent>> MapToProductsAsync(RepairedProduct repaired, DownloadedProduct downloaded, CancellationToken cancellationToken)
    {
        var result = new List<ProductsCurrent>();
        var code = await _codeGenerator.GenerateAsync(
            string.IsNullOrWhiteSpace(repaired.Brand) ? "UNK" : repaired.Brand,
            string.IsNullOrWhiteSpace(repaired.Category) ? "UNK" : repaired.Category,
            cancellationToken);

        repaired.Code = code;

        var price = downloaded.Variants.FirstOrDefault()?.CurrentPrice;
        var priceStandard = downloaded.Variants.FirstOrDefault()?.BasicPrice ?? price;

        var main = new ProductsCurrent
        {
            Kod = code,
            Url = downloaded.Url,
            UrlDomain = UrlDomain(downloaded.Url),
            IsActive = true,
            Nazev = repaired.Name,
            Popis = repaired.Desc,
            PopisStrucny = repaired.ShortDesc,
            Vyrobce = repaired.Brand,
            KategorieId = repaired.CategoryIds,
            HeurekaCzKategorie = repaired.HeurekaCategory,
            ZboziCzKategorie = repaired.ZboziCategory,
            GoogleKategorie = repaired.GoogleCategory,
            GlamiKategorie = repaired.GlamiCategory,
            ZboziCzProductname = repaired.Name,
            HeurekaCzProductname = repaired.Name,
            Cena = price,
            CenaBezna = priceStandard,
            Typ = "produkt"
        };

        result.Add(main);

        if (downloaded.Variants.Count == 0)
        {
            return result;
        }

        var index = 1;
        foreach (var variant in downloaded.Variants)
        {
            var variantCode = $"{code}-{index:00}";
            var optionPairs = variant.Options.Take(3).ToList();
            var variantRow = new ProductsCurrent
            {
                Kod = variantCode,
                Url = downloaded.Url,
                UrlDomain = UrlDomain(downloaded.Url),
                IsActive = true,
                Typ = "varianta",
                VariantaId = variantCode,
                Varianta1Nazev = optionPairs.Count > 0 ? optionPairs[0].OptionName : null,
                Varianta1Hodnota = optionPairs.Count > 0 ? optionPairs[0].OptionValue : null,
                Varianta2Nazev = optionPairs.Count > 1 ? optionPairs[1].OptionName : null,
                Varianta2Hodnota = optionPairs.Count > 1 ? optionPairs[1].OptionValue : null,
                Varianta3Nazev = optionPairs.Count > 2 ? optionPairs[2].OptionName : null,
                Varianta3Hodnota = optionPairs.Count > 2 ? optionPairs[2].OptionValue : null,
                Nazev = repaired.Name,
                Vyrobce = repaired.Brand,
                Cena = variant.CurrentPrice,
                CenaBezna = variant.BasicPrice ?? variant.CurrentPrice
            };

            result.Add(variantRow);
            index++;
        }

        return result;
    }

    private async Task UpsertProductsAsync(List<ProductsCurrent> products, CancellationToken cancellationToken)
    {
        foreach (var product in products)
        {
            var existing = await _db.ProductsCurrent.FirstOrDefaultAsync(x => x.Kod == product.Kod, cancellationToken);
            if (existing == null)
            {
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;
                _db.ProductsCurrent.Add(product);
            }
            else
            {
                CopyProduct(existing, product);
                existing.UpdatedAt = DateTime.UtcNow;
                _db.ProductsHistory.Add(BuildHistory(existing));
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static void CopyProduct(ProductsCurrent target, ProductsCurrent source)
    {
        target.Url = source.Url;
        target.UrlDomain = source.UrlDomain;
        target.IsActive = source.IsActive;
        target.IdOutput = source.IdOutput;
        target.Typ = source.Typ;
        target.VariantaId = source.VariantaId;
        target.Varianta1Nazev = source.Varianta1Nazev;
        target.Varianta1Hodnota = source.Varianta1Hodnota;
        target.Varianta2Nazev = source.Varianta2Nazev;
        target.Varianta2Hodnota = source.Varianta2Hodnota;
        target.Varianta3Nazev = source.Varianta3Nazev;
        target.Varianta3Hodnota = source.Varianta3Hodnota;
        target.VariantaStejne = source.VariantaStejne;
        target.Zobrazit = source.Zobrazit;
        target.Archiv = source.Archiv;
        target.KodVyrobku = source.KodVyrobku;
        target.Ean = source.Ean;
        target.Isbn = source.Isbn;
        target.Nazev = source.Nazev;
        target.Privlastek = source.Privlastek;
        target.Vyrobce = source.Vyrobce;
        target.DodavatelId = source.DodavatelId;
        target.Cena = source.Cena;
        target.CenaBezna = source.CenaBezna;
        target.CenaNakupni = source.CenaNakupni;
        target.RecyklacniPoplatek = source.RecyklacniPoplatek;
        target.Dph = source.Dph;
        target.Sleva = source.Sleva;
        target.SlevaOd = source.SlevaOd;
        target.SlevaDo = source.SlevaDo;
        target.Popis = source.Popis;
        target.PopisStrucny = source.PopisStrucny;
        target.Kosik = source.Kosik;
        target.Home = source.Home;
        target.Dostupnost = source.Dostupnost;
        target.DopravaZdarma = source.DopravaZdarma;
        target.DodaciDoba = source.DodaciDoba;
        target.DodaciDobaAuto = source.DodaciDobaAuto;
        target.Sklad = source.Sklad;
        target.NaSklade = source.NaSklade;
        target.Hmotnost = source.Hmotnost;
        target.Delka = source.Delka;
        target.Jednotka = source.Jednotka;
        target.OdberPo = source.OdberPo;
        target.OdberMin = source.OdberMin;
        target.OdberMax = source.OdberMax;
        target.Pocet = source.Pocet;
        target.Zaruka = source.Zaruka;
        target.MarzeDodavatel = source.MarzeDodavatel;
        target.SeoTitulek = source.SeoTitulek;
        target.SeoPopis = source.SeoPopis;
        target.Eroticke = source.Eroticke;
        target.ProDospele = source.ProDospele;
        target.SlevovyKupon = source.SlevovyKupon;
        target.DarekObjednavka = source.DarekObjednavka;
        target.Priorita = source.Priorita;
        target.Poznamka = source.Poznamka;
        target.DodavatelKod = source.DodavatelKod;
        target.Stitky = source.Stitky;
        target.CenaDodavatel = source.CenaDodavatel;
        target.KategorieId = source.KategorieId;
        target.Podobne = source.Podobne;
        target.Prislusenstvi = source.Prislusenstvi;
        target.Variantove = source.Variantove;
        target.Zdarma = source.Zdarma;
        target.Sluzby = source.Sluzby;
        target.RozsirujiciObsah = source.RozsirujiciObsah;
        target.ZboziCzSkryt = source.ZboziCzSkryt;
        target.ZboziCzProductname = source.ZboziCzProductname;
        target.ZboziCzProduct = source.ZboziCzProduct;
        target.ZboziCzCpc = source.ZboziCzCpc;
        target.ZboziCzCpcSearch = source.ZboziCzCpcSearch;
        target.ZboziCzKategorie = source.ZboziCzKategorie;
        target.ZboziCzStitek0 = source.ZboziCzStitek0;
        target.ZboziCzStitek1 = source.ZboziCzStitek1;
        target.ZboziCzExtra = source.ZboziCzExtra;
        target.HeurekaCzSkryt = source.HeurekaCzSkryt;
        target.HeurekaCzProductname = source.HeurekaCzProductname;
        target.HeurekaCzProduct = source.HeurekaCzProduct;
        target.HeurekaCzCpc = source.HeurekaCzCpc;
        target.HeurekaCzKategorie = source.HeurekaCzKategorie;
        target.GoogleSkryt = source.GoogleSkryt;
        target.GoogleKategorie = source.GoogleKategorie;
        target.GoogleStitek0 = source.GoogleStitek0;
        target.GoogleStitek1 = source.GoogleStitek1;
        target.GoogleStitek2 = source.GoogleStitek2;
        target.GoogleStitek3 = source.GoogleStitek3;
        target.GoogleStitek4 = source.GoogleStitek4;
        target.GlamiSkryt = source.GlamiSkryt;
        target.GlamiKategorie = source.GlamiKategorie;
        target.GlamiCpc = source.GlamiCpc;
        target.GlamiVoucher = source.GlamiVoucher;
        target.GlamiMaterial = source.GlamiMaterial;
        target.GlamiSkMaterial = source.GlamiSkMaterial;
        target.SkladUmisteni = source.SkladUmisteni;
        target.SkladMinimalni = source.SkladMinimalni;
        target.SkladOptimalni = source.SkladOptimalni;
        target.SkladMaximalni = source.SkladMaximalni;
    }

    private static ProductsHistory BuildHistory(ProductsCurrent product)
    {
        return new ProductsHistory
        {
            ProductId = product.Id,
            ValidFrom = DateTime.UtcNow,
            ChangeSource = "unifier",
            Kod = product.Kod,
            Url = product.Url,
            UrlDomain = product.UrlDomain,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            IdOutput = product.IdOutput,
            Typ = product.Typ,
            VariantaId = product.VariantaId,
            Varianta1Nazev = product.Varianta1Nazev,
            Varianta1Hodnota = product.Varianta1Hodnota,
            Varianta2Nazev = product.Varianta2Nazev,
            Varianta2Hodnota = product.Varianta2Hodnota,
            Varianta3Nazev = product.Varianta3Nazev,
            Varianta3Hodnota = product.Varianta3Hodnota,
            VariantaStejne = product.VariantaStejne,
            Zobrazit = product.Zobrazit,
            Archiv = product.Archiv,
            KodVyrobku = product.KodVyrobku,
            Ean = product.Ean,
            Isbn = product.Isbn,
            Nazev = product.Nazev,
            Privlastek = product.Privlastek,
            Vyrobce = product.Vyrobce,
            DodavatelId = product.DodavatelId,
            Cena = product.Cena,
            CenaBezna = product.CenaBezna,
            CenaNakupni = product.CenaNakupni,
            RecyklacniPoplatek = product.RecyklacniPoplatek,
            Dph = product.Dph,
            Sleva = product.Sleva,
            SlevaOd = product.SlevaOd,
            SlevaDo = product.SlevaDo,
            Popis = product.Popis,
            PopisStrucny = product.PopisStrucny,
            Kosik = product.Kosik,
            Home = product.Home,
            Dostupnost = product.Dostupnost,
            DopravaZdarma = product.DopravaZdarma,
            DodaciDoba = product.DodaciDoba,
            DodaciDobaAuto = product.DodaciDobaAuto,
            Sklad = product.Sklad,
            NaSklade = product.NaSklade,
            Hmotnost = product.Hmotnost,
            Delka = product.Delka,
            Jednotka = product.Jednotka,
            OdberPo = product.OdberPo,
            OdberMin = product.OdberMin,
            OdberMax = product.OdberMax,
            Pocet = product.Pocet,
            Zaruka = product.Zaruka,
            MarzeDodavatel = product.MarzeDodavatel,
            SeoTitulek = product.SeoTitulek,
            SeoPopis = product.SeoPopis,
            Eroticke = product.Eroticke,
            ProDospele = product.ProDospele,
            SlevovyKupon = product.SlevovyKupon,
            DarekObjednavka = product.DarekObjednavka,
            Priorita = product.Priorita,
            Poznamka = product.Poznamka,
            DodavatelKod = product.DodavatelKod,
            Stitky = product.Stitky,
            CenaDodavatel = product.CenaDodavatel,
            KategorieId = product.KategorieId,
            Podobne = product.Podobne,
            Prislusenstvi = product.Prislusenstvi,
            Variantove = product.Variantove,
            Zdarma = product.Zdarma,
            Sluzby = product.Sluzby,
            RozsirujiciObsah = product.RozsirujiciObsah,
            ZboziCzSkryt = product.ZboziCzSkryt,
            ZboziCzProductname = product.ZboziCzProductname,
            ZboziCzProduct = product.ZboziCzProduct,
            ZboziCzCpc = product.ZboziCzCpc,
            ZboziCzCpcSearch = product.ZboziCzCpcSearch,
            ZboziCzKategorie = product.ZboziCzKategorie,
            ZboziCzStitek0 = product.ZboziCzStitek0,
            ZboziCzStitek1 = product.ZboziCzStitek1,
            ZboziCzExtra = product.ZboziCzExtra,
            HeurekaCzSkryt = product.HeurekaCzSkryt,
            HeurekaCzProductname = product.HeurekaCzProductname,
            HeurekaCzProduct = product.HeurekaCzProduct,
            HeurekaCzCpc = product.HeurekaCzCpc,
            HeurekaCzKategorie = product.HeurekaCzKategorie,
            GoogleSkryt = product.GoogleSkryt,
            GoogleKategorie = product.GoogleKategorie,
            GoogleStitek0 = product.GoogleStitek0,
            GoogleStitek1 = product.GoogleStitek1,
            GoogleStitek2 = product.GoogleStitek2,
            GoogleStitek3 = product.GoogleStitek3,
            GoogleStitek4 = product.GoogleStitek4,
            GlamiSkryt = product.GlamiSkryt,
            GlamiKategorie = product.GlamiKategorie,
            GlamiCpc = product.GlamiCpc,
            GlamiVoucher = product.GlamiVoucher,
            GlamiMaterial = product.GlamiMaterial,
            GlamiSkMaterial = product.GlamiSkMaterial,
            SkladUmisteni = product.SkladUmisteni,
            SkladMinimalni = product.SkladMinimalni,
            SkladOptimalni = product.SkladOptimalni,
            SkladMaximalni = product.SkladMaximalni
        };
    }

    private static UnifierApproval BuildApproval(RepairedProduct repaired, string propertyName, string? suggestedValue)
    {
        return new UnifierApproval
        {
            ProductCode = string.IsNullOrWhiteSpace(repaired.Code) ? repaired.OriginalName : repaired.Code,
            Url = repaired.Url,
            ImageUrl = string.Empty,
            PropertyName = propertyName,
            CurrentValue = string.Empty,
            SuggestedValue = suggestedValue,
            Language = "cs",
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string UrlDomain(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.Host.ToLowerInvariant();
        }

        return string.Empty;
    }
}

public sealed record UnifierProcessingResult(UnifierProcessOutcome Outcome, IReadOnlyList<ProductsCurrent> Products, IReadOnlyList<UnifierApproval> Approvals);

public enum UnifierProcessOutcome
{
    Success = 0,
    WaitingApproval = 1,
    Error = 2
}
