using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Repositories;

namespace Lykke.Snow.PriceAlerts.Domain.Services
{
    public interface IPriceAlertsCache : ICache, IPriceAlertsRepository
    {
        bool IsUnique(PriceAlert priceAlert);
        bool IsActive(string id, out PriceAlert cachedAlert);
    }
}