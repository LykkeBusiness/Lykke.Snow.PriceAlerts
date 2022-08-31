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

        Task<PaginatedResponse<PriceAlert>> GetByPageAsync(string accountId, string productId, AlertStatus[] statuses,
            int skip, int take);
        Task<int> CancelByProductAndAccountAsync(string productId = null, string accountId = null);
        Task<Result<PriceAlertErrorCodes>> TriggerAsync(string id);
        Task ExpireAllAsync(DateTime expirationDate);
        Task<Dictionary<string, int>> GetActiveCountAsync(List<string> productIds, string accountId);
    }
}