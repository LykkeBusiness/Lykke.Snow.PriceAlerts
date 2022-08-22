using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Snow.Common.Model;
using Lykke.Snow.PriceAlerts.Domain.Models;

namespace Lykke.Snow.PriceAlerts.Domain.Services
{
    public interface IPriceAlertsService
    {
        Task<Result<PriceAlert, PriceAlertErrorCodes>> GetByIdAsync(string id);
        Task<Result<PriceAlertErrorCodes>> InsertAsync(PriceAlert priceAlert);
        Task<Result<PriceAlertErrorCodes>> UpdateAsync(PriceAlert priceAlert);
        Task<Result<PriceAlertErrorCodes>> CancelAsync(string id);

        Task<PaginatedResponse<PriceAlert>> GetByPageAsync(string accountId, string productId, AlertStatus? status,
            int skip, int take);

        Task CancelByProductIdAsync(string productId);
        Task<int> CancelByProductIdAsync(string productId, string accountId);
        ValueTask<IEnumerable<PriceAlert>> GetActiveByProductIdAsync(string productId);
        Task<Result<PriceAlertErrorCodes>> TriggerAsync(string id);
        Task ExpireAllAsync(DateTime expirationDate);
        Task<Dictionary<string, int>> GetActiveCountAsync(List<string> productIds, string accountId);
        ValueTask<IEnumerable<PriceAlert>> GetActiveByAccountIdAsync(string accountId);
    }
}