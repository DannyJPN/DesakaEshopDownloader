using Desaka.Contracts.Unifier;

namespace Desaka.Unifier.Infrastructure;

/// <summary>
/// Combines products from different sources and detects changes.
/// </summary>
public interface IProductCombiner
{
    CombineResult Combine(
        IEnumerable<ExportProductDTO> existingProducts,
        IEnumerable<ExportProductDTO> newProducts);
}

public record CombineResult(
    List<ExportProductDTO> CombinedProducts,
    List<ProductChange> NewProducts,
    List<ProductChange> RemovedProducts,
    List<CodeChange> CodeChanges,
    List<PriceChange> PriceIncreases,
    List<PriceChange> PriceDecreases
);

public record ProductChange(string Code, string Name, string Url);
public record CodeChange(string Name, string OldCode, string NewCode, string Url);
public record PriceChange(string Name, string Code, decimal OldPrice, decimal NewPrice, decimal Difference, double Percentage, string Url);

public sealed class ProductCombiner : IProductCombiner
{
    public CombineResult Combine(
        IEnumerable<ExportProductDTO> existingProducts,
        IEnumerable<ExportProductDTO> newProducts)
    {
        var existingByName = CreateNameIndex(existingProducts);
        var newByName = CreateNameIndex(newProducts);

        var combined = new List<ExportProductDTO>();
        var newProductChanges = new List<ProductChange>();
        var removedProducts = new List<ProductChange>();
        var codeChanges = new List<CodeChange>();
        var priceIncreases = new List<PriceChange>();
        var priceDecreases = new List<PriceChange>();
        var processedExistingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Process new products
        foreach (var (nameLower, newProds) in newByName)
        {
            if (existingByName.TryGetValue(nameLower, out var existingProds))
            {
                // Product exists in both - check for changes
                var existingMain = existingProds.FirstOrDefault();
                var newMain = newProds.FirstOrDefault();

                if (existingMain != null && newMain != null)
                {
                    // Check code changes
                    if (!string.IsNullOrEmpty(existingMain.Code) && !string.IsNullOrEmpty(newMain.Code) &&
                        existingMain.Code != newMain.Code)
                    {
                        codeChanges.Add(new CodeChange(newMain.Name, existingMain.Code, newMain.Code, newMain.Url));
                    }

                    // Check price changes
                    if (existingMain.Price > 0 && newMain.Price > 0 && existingMain.Price != newMain.Price)
                    {
                        var diff = newMain.Price - existingMain.Price;
                        var pct = (double)((newMain.Price - existingMain.Price) / existingMain.Price * 100);

                        var change = new PriceChange(newMain.Name, newMain.Code, existingMain.Price, newMain.Price, diff, pct, newMain.Url);

                        if (newMain.Price > existingMain.Price)
                            priceIncreases.Add(change);
                        else
                            priceDecreases.Add(change);
                    }
                }

                combined.AddRange(newProds);
                processedExistingNames.Add(nameLower);
            }
            else
            {
                // New product - not in existing
                combined.AddRange(newProds);
                foreach (var p in newProds)
                    newProductChanges.Add(new ProductChange(p.Code, p.Name, p.Url));
            }
        }

        // Find removed products (in existing but not in new)
        foreach (var (nameLower, existingProds) in existingByName)
        {
            if (!processedExistingNames.Contains(nameLower))
            {
                foreach (var p in existingProds)
                    removedProducts.Add(new ProductChange(p.Code, p.Name, p.Url));
            }
        }

        return new CombineResult(combined, newProductChanges, removedProducts, codeChanges, priceIncreases, priceDecreases);
    }

    private static Dictionary<string, List<ExportProductDTO>> CreateNameIndex(IEnumerable<ExportProductDTO> products)
    {
        var index = new Dictionary<string, List<ExportProductDTO>>(StringComparer.OrdinalIgnoreCase);

        foreach (var product in products)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                continue;

            var key = product.Name.Trim();
            if (!index.TryGetValue(key, out var list))
            {
                list = new List<ExportProductDTO>();
                index[key] = list;
            }
            list.Add(product);
        }

        return index;
    }
}
