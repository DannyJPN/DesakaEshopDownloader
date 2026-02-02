namespace Desaka.Scraping.Models;

public enum ScrapeContentType
{
    Html = 0,
    Json = 1
}

public sealed class ScrapeRequest
{
    public int EshopId { get; set; }
    public string Url { get; set; } = "";
    public ScrapeContentType ContentType { get; set; } = ScrapeContentType.Html;
    public string? UserAgent { get; set; }
}

public sealed class ScrapeContent
{
    public string Url { get; set; } = "";
    public ScrapeContentType ContentType { get; set; }
    public byte[] Body { get; set; } = Array.Empty<byte>();
    public string Hash { get; set; } = "";
    public DateTime DownloadedAt { get; set; }
}

public sealed class ScrapeResult
{
    public ScrapeContent Content { get; set; } = new();
    public ScrapedProduct? Product { get; set; }
}

public sealed class ScrapedProduct
{
    public string Name { get; set; } = "";
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string Url { get; set; } = "";
    public string? MainPhotoUrl { get; set; }
    public List<string> GalleryUrls { get; } = new();
    public List<ScrapedVariant> Variants { get; } = new();
}

public sealed class ScrapedVariant
{
    public decimal? CurrentPrice { get; set; }
    public decimal? BasicPrice { get; set; }
    public string? StockStatus { get; set; }
    public List<ScrapedVariantOption> Options { get; } = new();
}

public sealed class ScrapedVariantOption
{
    public string OptionName { get; set; } = "";
    public string OptionValue { get; set; } = "";
}
