using Lykke.Snow.PriceAlerts.Domain.Models;

namespace Lykke.Snow.PriceAlerts.Domain.Services
{
    public interface IProductsCache : ICache
    {
        bool Contains(string productId);
        void Remove(string productId);
        void AddOrUpdate(ProductCacheModel cacheModel);
        ProductCacheModel Get(string productId);
    }
}