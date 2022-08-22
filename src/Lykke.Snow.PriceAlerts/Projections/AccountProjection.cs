using System.Linq;
using System.Threading.Tasks;
using Lykke.Snow.PriceAlerts.Domain.Services;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AssetService.Contracts;
using MarginTrading.AssetService.Contracts.ErrorCodes;
using MarginTrading.AssetService.Contracts.TradingConditions;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace Lykke.Snow.PriceAlerts.Projections
{
    public class AccountProjection
    {
        private readonly IPriceAlertsService _priceAlertsService;
        private readonly ITradingInstrumentsApi _tradingInstrumentsApi;
        private readonly ILogger<AccountProjection> _logger;

        public AccountProjection(IPriceAlertsService priceAlertsService,
            ITradingInstrumentsApi tradingInstrumentsApi,
            ILogger<AccountProjection> logger)
        {
            _priceAlertsService = priceAlertsService;
            _tradingInstrumentsApi = tradingInstrumentsApi;
            _logger = logger;
        }

        public async Task Handle(AccountChangedEvent e)
        {
            if (!(e.EventType == AccountChangedEventTypeContract.Updated
                  && e.Source == "UpdateClientTradingCondition"))
            {
                return;
            }

            var tradingConditionId = e.Account.TradingConditionId;
            var accountId = e.Account.Id;

            var alerts = await _priceAlertsService.GetActiveByAccountIdAsync(accountId);
            var products = alerts.Select(x => x.ProductId).Distinct();

            var unavailableProductsResponse = await _tradingInstrumentsApi.CheckProductsUnavailableForTradingCondition(
                new CheckProductsUnavailableForTradingConditionRequest()
                {
                    ProductIds = products.ToList(),
                    TradingConditionId = tradingConditionId,
                });

            if (unavailableProductsResponse.ErrorCode != ClientProfilesErrorCodesContract.None)
            {
                _logger.LogWarning(
                    "Cannot retrieve a list of unavailable products for accountId {AccountId}, trading condition id {TradingConditionId}",
                    accountId,
                    tradingConditionId);
                return;
            }

            _logger.LogInformation("{N} unavailable products found for accountId {AccountId}",
                unavailableProductsResponse.UnavailableProductIds.Count,
                accountId);
            foreach (var productId in unavailableProductsResponse.UnavailableProductIds)
            {
                await _priceAlertsService.CancelByProductIdAsync(productId, accountId);
            }
        }
    }
}