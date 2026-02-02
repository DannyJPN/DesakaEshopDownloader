namespace Desaka.Unifying.Models;

public sealed class DownloadedProduct
{
    public long? Id { get; set; }
    public int EshopId { get; set; }
    public string Name { get; set; } = "";
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? MainPhotoPath { get; set; }
    public string Url { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public List<string> GalleryFilepaths { get; } = new();
    public List<DownloadedVariant> Variants { get; } = new();
}

public sealed class DownloadedVariant
{
    public long? Id { get; set; }
    public decimal? CurrentPrice { get; set; }
    public decimal? BasicPrice { get; set; }
    public string? StockStatus { get; set; }
    public List<DownloadedVariantOption> Options { get; } = new();
}

public sealed class DownloadedVariantOption
{
    public string OptionName { get; set; } = "";
    public string OptionValue { get; set; } = "";
}
