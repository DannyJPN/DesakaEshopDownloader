namespace Desaka.Unifying.Models;

public sealed class RepairedProduct
{
    public string OriginalName { get; set; } = "";
    public string Category { get; set; } = "";
    public string Brand { get; set; } = "";
    public string Type { get; set; } = "";
    public string Model { get; set; } = "";
    public string CategoryIds { get; set; } = "";
    public string Code { get; set; } = "";
    public string Desc { get; set; } = "";
    public string GlamiCategory { get; set; } = "";
    public string GoogleCategory { get; set; } = "";
    public string GoogleKeywords { get; set; } = "";
    public string HeurekaCategory { get; set; } = "";
    public string Name { get; set; } = "";
    public string Price { get; set; } = "";
    public string PriceStandard { get; set; } = "";
    public string ShortDesc { get; set; } = "";
    public string Url { get; set; } = "";
    public List<Variant> Variants { get; } = new();
    public string ZboziCategory { get; set; } = "";
    public string ZboziKeywords { get; set; } = "";
}
