using Desaka.Contracts.Common;
using Desaka.Contracts.EventBus;
using Desaka.Contracts.WebScrape;
using Desaka.DataAccess;
using Desaka.DataAccess.Entities;
using Desaka.EventBus;
using Desaka.Scraping.Models;
using Desaka.Scraping.Services;
using Desaka.WebScrape.Application;
using Microsoft.EntityFrameworkCore;

namespace Desaka.WebScrape.Infrastructure;

public sealed class WebScrapeService : IWebScrapeService
{
    private readonly DesakaDbContext _db;
    private readonly IEventBus _eventBus;
    private readonly IFileService _fileService;
    private readonly HttpClient _httpClient;
    private readonly IRuntimeLockService _lockService;
    private readonly ISecretProtector _secretProtector;
    private readonly ISiteScraper _siteScraper;

    public WebScrapeService(
        DesakaDbContext db,
        IEventBus eventBus,
        IFileService fileService,
        HttpClient httpClient,
        IRuntimeLockService lockService,
        ISecretProtector secretProtector,
        ISiteScraper siteScraper)
    {
        _db = db;
        _eventBus = eventBus;
        _fileService = fileService;
        _httpClient = httpClient;
        _lockService = lockService;
        _secretProtector = secretProtector;
        _siteScraper = siteScraper;
    }

    public async Task<WebScrapeStartResponseDTO> StartAsync(WebScrapeStartRequestDTO request, CancellationToken cancellationToken = default)
    {
        var existing = await _db.ScrapeRuns
            .Where(x => x.EshopId == request.EshopId && x.Status == "running")
            .OrderByDescending(x => x.StartAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null)
        {
            return new WebScrapeStartResponseDTO(existing.Id, JobStatus.Running);
        }

        var run = new ScrapeRun
        {
            EshopId = request.EshopId,
            StartAt = DateTime.UtcNow,
            Status = "running"
        };

        _db.ScrapeRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        var lockKey = $"webscrape:eshop:{run.EshopId}";
        var ownerId = $"webscrape:{run.Id}";
        var acquired = await _lockService.TryAcquireAsync(lockKey, ownerId, TimeSpan.FromHours(3), cancellationToken);
        if (!acquired)
        {
            run.Status = "cancelled";
            run.ErrorMessage = "Runtime lock busy.";
            run.EndAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return new WebScrapeStartResponseDTO(run.Id, JobStatus.Cancelled);
        }

        await DownloadRawAsync(run, request.Force, cancellationToken);

        await _lockService.ReleaseAsync(lockKey, ownerId, cancellationToken);

        await _eventBus.PublishAsync(new EventEnvelope(
            "WebScrapeStarted",
            "WebScrape",
            DateTime.UtcNow,
            request.CorrelationId,
            System.Text.Json.JsonSerializer.Serialize(new { run.Id, run.EshopId })),
            cancellationToken);

        return new WebScrapeStartResponseDTO(run.Id, JobStatus.Running);
    }

    private async Task DownloadRawAsync(ScrapeRun run, bool force, CancellationToken cancellationToken)
    {
        try
        {
            var eshop = await _db.ConfigEshops.AsNoTracking().FirstOrDefaultAsync(x => x.Id == run.EshopId, cancellationToken);
            if (eshop == null)
            {
                run.Status = "error";
                run.ErrorMessage = "Eshop not found.";
                run.EndAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
                return;
            }

            if (IsPincesobchod(eshop.BaseUrl))
            {
                await DownloadPincesobchodAsync(eshop, force, cancellationToken);
            }
            else
            {
                await DownloadFromSitemapAsync(eshop, force, cancellationToken);
            }

            run.Status = "success";
            run.EndAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            run.Status = "error";
            run.ErrorMessage = ex.Message;
            run.EndAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<string> ResolvePathAsync(string key, string fallback, CancellationToken cancellationToken)
    {
        var setting = await _db.ConfigSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        return string.IsNullOrWhiteSpace(setting?.Value) ? fallback : setting.Value;
    }

    private async Task DownloadSinglePageAsync(ConfigEshop eshop, bool force, CancellationToken cancellationToken)
    {
        var url = eshop.BaseUrl;
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var rawResult = await WriteRawIfChangedAsync(eshop.Id, url, body, "html", force, cancellationToken);

        var extractor = new NullExtractor();
        var scraper = new HttpScraper(_httpClient, extractor);
        var scrapeResult = await scraper.ScrapeAsync(new ScrapeRequest
        {
            EshopId = eshop.Id,
            Url = url,
            ContentType = ScrapeContentType.Html
        }, cancellationToken);

        if (scrapeResult.Product != null)
        {
            await SaveDownloadedProductsAsync(eshop.Id, new[] { scrapeResult.Product }, cancellationToken);
        }

        if (rawResult != null)
        {
            _db.ScrapeRawMetadata.Add(rawResult);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task DownloadFromSitemapAsync(ConfigEshop eshop, bool force, CancellationToken cancellationToken)
    {
        var batch = await _siteScraper.ScrapeAsync(new SiteScrapeRequest
        {
            BaseUrl = eshop.BaseUrl,
            UserAgent = null,
            MaxProducts = 500
        }, cancellationToken);

        foreach (var item in batch)
        {
            if (item.Product == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.Product.Url))
            {
                item.Product.Url = item.Content.Url;
            }

            if (item.Content.Body.Length > 0)
            {
                var metadata = await WriteRawIfChangedAsync(eshop.Id, item.Content.Url, item.Content.Body, "html", force, cancellationToken);
                if (metadata != null)
                {
                    _db.ScrapeRawMetadata.Add(metadata);
                }
            }

            if (item.Product != null)
            {
                await SaveDownloadedProductsAsync(eshop.Id, new[] { item.Product }, cancellationToken);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task DownloadPincesobchodAsync(ConfigEshop eshop, bool force, CancellationToken cancellationToken)
    {
        var keys = await _db.ConfigPincesobchodApiKeys.AsNoTracking()
            .Where(x => x.IsEnabled)
            .ToListAsync(cancellationToken);

        if (keys.Count == 0)
        {
            keys.Add(new ConfigPincesobchodApiKey { LanguageCode = "cs", ApiKeyEncrypted = string.Empty, IsEnabled = true });
        }

        foreach (var key in keys)
        {
            var url = $"{eshop.BaseUrl.TrimEnd('/')}/api/v1/products?page=1";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(key.ApiKeyEncrypted))
            {
                var token = _secretProtector.Unprotect(key.ApiKeyEncrypted);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            var rawResult = await WriteRawIfChangedAsync(eshop.Id, url, body, "json", force, cancellationToken);
            if (rawResult != null)
            {
                _db.ScrapeRawMetadata.Add(rawResult);
                await _db.SaveChangesAsync(cancellationToken);
            }

            var products = PincesobchodCatalogParser.Parse(body, key.LanguageCode);
            await SaveDownloadedProductsAsync(eshop.Id, products, cancellationToken);
        }
    }

    private async Task<ScrapeRawMetadata?> WriteRawIfChangedAsync(int eshopId, string url, byte[] body, string contentType, bool force, CancellationToken cancellationToken)
    {
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(body)).ToLowerInvariant();
        var existing = await _db.ScrapeRawMetadata.AsNoTracking()
            .Where(x => x.Url == url)
            .OrderByDescending(x => x.DownloadedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null && existing.ContentHash == hash && !force)
        {
            return null;
        }

        var rawRoot = await ResolvePathAsync("paths.raw_root", "data/raw", cancellationToken);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var extension = contentType == "json" ? "json" : "html";
        var fileName = $"{eshopId}_{timestamp}.{extension}";
        var relativePath = Path.Combine(eshopId.ToString(), fileName);
        var fileResult = await _fileService.WriteBytesAsync(rawRoot, relativePath, body, cancellationToken);

        return new ScrapeRawMetadata
        {
            EshopId = eshopId,
            Url = url,
            FilePath = fileResult.RelativePath,
            ContentHash = hash,
            ContentSize = body.Length,
            ContentType = contentType,
            DownloadedAt = DateTime.UtcNow
        };
    }

    private async Task SaveDownloadedProductsAsync(int eshopId, IEnumerable<ScrapedProduct> products, CancellationToken cancellationToken)
    {
        foreach (var product in products)
        {
            if (string.IsNullOrWhiteSpace(product.Name) && string.IsNullOrWhiteSpace(product.Url))
            {
                continue;
            }

            var entity = new DownloadedProduct
            {
                EshopId = eshopId,
                Name = product.Name ?? string.Empty,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                MainPhotoPath = await DownloadImageAsync(product.MainPhotoUrl, eshopId, cancellationToken),
                Url = product.Url ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _db.DownloadedProducts.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            foreach (var galleryUrl in product.GalleryUrls)
            {
                var path = await DownloadImageAsync(galleryUrl, eshopId, cancellationToken);
                if (!string.IsNullOrWhiteSpace(path))
                {
                    _db.DownloadedGalleries.Add(new DownloadedGallery
                    {
                        ProductId = entity.Id,
                        Filepath = path
                    });
                }
            }

            foreach (var variant in product.Variants)
            {
                var variantEntity = new DownloadedVariant
                {
                    ProductId = entity.Id,
                    CurrentPrice = variant.CurrentPrice,
                    BasicPrice = variant.BasicPrice,
                    StockStatus = variant.StockStatus
                };

                _db.DownloadedVariants.Add(variantEntity);
                await _db.SaveChangesAsync(cancellationToken);

                foreach (var option in variant.Options)
                {
                    _db.DownloadedVariantOptions.Add(new DownloadedVariantOption
                    {
                        VariantId = variantEntity.Id,
                        OptionName = option.OptionName,
                        OptionValue = option.OptionValue
                    });
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<string?> DownloadImageAsync(string? url, int eshopId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var imagesRoot = await ResolvePathAsync("paths.images_root", "data/images", cancellationToken);
        var ext = Path.GetExtension(new Uri(url).AbsolutePath);
        if (string.IsNullOrWhiteSpace(ext))
        {
            ext = ".jpg";
        }

        var fileName = $"{eshopId}_{Guid.NewGuid():N}{ext}";
        var relativePath = Path.Combine(eshopId.ToString(), fileName);
        var result = await _fileService.WriteBytesAsync(imagesRoot, relativePath, bytes, cancellationToken);
        return result.RelativePath;
    }

    private static bool IsPincesobchod(string baseUrl)
        => baseUrl.Contains("pincesobchod", StringComparison.OrdinalIgnoreCase);

    public async Task<WebScrapeStatusResponseDTO?> GetStatusAsync(long? runId, int? eshopId, CancellationToken cancellationToken = default)
    {
        var query = _db.ScrapeRuns.AsNoTracking().AsQueryable();
        if (runId.HasValue)
        {
            query = query.Where(x => x.Id == runId.Value);
        }
        else if (eshopId.HasValue)
        {
            query = query.Where(x => x.EshopId == eshopId.Value)
                .OrderByDescending(x => x.StartAt)
                .Take(1);
        }
        else
        {
            return null;
        }

        var run = await query.FirstOrDefaultAsync(cancellationToken);
        if (run == null)
        {
            return null;
        }

        return new WebScrapeStatusResponseDTO(run.Id, ParseStatus(run.Status), 0, run.StartAt, run.EndAt ?? DateTime.UtcNow, run.ErrorMessage);
    }

    public async Task<IReadOnlyList<WebScrapeStatusResponseDTO>> ListAsync(string? status, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = _db.ScrapeRuns.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.StartAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.StartAt <= to.Value);
        }

        var runs = await query.OrderByDescending(x => x.StartAt).ToListAsync(cancellationToken);
        return runs.Select(run => new WebScrapeStatusResponseDTO(run.Id, ParseStatus(run.Status), 0, run.StartAt, run.EndAt ?? DateTime.UtcNow, run.ErrorMessage)).ToList();
    }

    private static JobStatus ParseStatus(string status)
        => status switch
        {
            "running" => JobStatus.Running,
            "success" => JobStatus.Success,
            "error" => JobStatus.Error,
            "cancelled" => JobStatus.Cancelled,
            _ => JobStatus.Queued
        };
}

