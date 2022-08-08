using System.Threading.Tasks;
using Lykke.Snow.PriceAlerts.Domain.Models;

namespace Lykke.Snow.PriceAlerts.Domain.Services
{
    public interface IQuoteCache : ICache
    {
        Task AddOrUpdate(QuoteCacheModel quote);
    }
}