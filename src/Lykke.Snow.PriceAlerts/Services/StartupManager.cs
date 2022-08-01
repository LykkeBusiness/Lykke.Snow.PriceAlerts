using System.Threading.Tasks;
using Lykke.Snow.PriceAlerts.Domain.Services;

namespace Lykke.Snow.PriceAlerts.Services
{
    public class StartupManager
    {
        private readonly IPriceAlertsCache _priceAlertsCache;
        private readonly IProductsCache _productsCache;

        public StartupManager(IProductsCache productsCache,
            IPriceAlertsCache priceAlertsCache)
        {
            _productsCache = productsCache;
            _priceAlertsCache = priceAlertsCache;
        }

        internal async Task Start()
        {
            await _productsCache.Init();
            await _priceAlertsCache.Init();
        }
    }
}