using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Snow.Common.Model;
using Lykke.Snow.PriceAlerts.Domain.Models;

namespace Lykke.Snow.PriceAlerts.Domain.Repositories
{
    public interface IPriceAlertsRepository
    {
        Task<Result<PriceAlert, PriceAlertErrorCodes>> GetByIdAsync(string id);
        Task<Result<PriceAlertErrorCodes>> InsertAsync(PriceAlert priceAlert);
        Task<Result<PriceAlertErrorCodes>> UpdateAsync(PriceAlert priceAlert);

        Task<PaginatedResponse<PriceAlert>> GetByPageAsync(string accountId, string productId, AlertStatus? status,
            int skip, int take);

        Task<List<PriceAlert>> GetAllActiveAlerts();
        Task<IEnumerable<PriceAlert>> GetActiveByProductId(string productId);
    }
}