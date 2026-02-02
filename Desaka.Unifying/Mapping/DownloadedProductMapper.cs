using Desaka.DataAccess.Entities;
using Desaka.Unifying.Models;
using EntityDownloadedProduct = Desaka.DataAccess.Entities.DownloadedProduct;
using EntityDownloadedVariant = Desaka.DataAccess.Entities.DownloadedVariant;
using EntityDownloadedVariantOption = Desaka.DataAccess.Entities.DownloadedVariantOption;
using EntityDownloadedGallery = Desaka.DataAccess.Entities.DownloadedGallery;
using ModelDownloadedProduct = Desaka.Unifying.Models.DownloadedProduct;

namespace Desaka.Unifying.Mapping;

public static class DownloadedProductMapper
{
    public static ModelDownloadedProduct Map(EntityDownloadedProduct entity, IReadOnlyList<EntityDownloadedGallery> galleries, IReadOnlyList<EntityDownloadedVariant> variants, IReadOnlyList<EntityDownloadedVariantOption> options)
    {
        var product = new ModelDownloadedProduct
        {
            Id = entity.Id,
            EshopId = entity.EshopId,
            Name = entity.Name,
            ShortDescription = entity.ShortDescription,
            Description = entity.Description,
            MainPhotoPath = entity.MainPhotoPath,
            Url = entity.Url,
            CreatedAt = entity.CreatedAt
        };

        foreach (var gallery in galleries)
        {
            product.GalleryFilepaths.Add(gallery.Filepath);
        }

        var optionsByVariant = options.GroupBy(x => x.VariantId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var variant in variants)
        {
            var mapped = new Models.DownloadedVariant
            {
                Id = variant.Id,
                CurrentPrice = variant.CurrentPrice,
                BasicPrice = variant.BasicPrice,
                StockStatus = variant.StockStatus
            };

            if (optionsByVariant.TryGetValue(variant.Id, out var variantOptions))
            {
                foreach (var option in variantOptions)
                {
                    mapped.Options.Add(new Models.DownloadedVariantOption
                    {
                        OptionName = option.OptionName,
                        OptionValue = option.OptionValue
                    });
                }
            }

            product.Variants.Add(mapped);
        }

        return product;
    }
}
