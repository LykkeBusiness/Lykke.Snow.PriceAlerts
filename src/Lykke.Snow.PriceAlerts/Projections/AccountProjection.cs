using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Snow.PriceAlerts.Domain.Models.InternalCommands;
using Lykke.Snow.PriceAlerts.Domain.Services;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.AssetService.Contracts;
using MarginTrading.AssetService.Contracts.ErrorCodes;
using MarginTrading.AssetService.Contracts.TradingConditions;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.Projections
{
    public class AccountProjection
    {
        private readonly IPriceAlertsCache _priceAlertsCache;
        private readonly ITradingInstrumentsApi _tradingInstrumentsApi;
        private readonly IObserver<CancelPriceAlertsCommand> _observer;
        private readonly ILogger<AccountProjection> _logger;

        public AccountProjection(IPriceAlertsCache priceAlertsCache,
            ITradingInstrumentsApi tradingInstrumentsApi,
            IObserver<CancelPriceAlertsCommand> observer,
            ILogger<AccountProjection> logger)
        {
            _priceAlertsCache = priceAlertsCache;
            _tradingInstrumentsApi = tradingInstrumentsApi;
            _observer = observer;
            _logger = logger;
        }

        [UsedImplicitly]
        public async Task Handle(AccountChangedEvent e)
        {
            switch (e.EventType)
            {
                case AccountChangedEventTypeContract.Created:
                    break;
                case AccountChangedEventTypeContract.Updated:
                    await HandleUpdated(e);
                    break;
                case AccountChangedEventTypeContract.BalanceUpdated:
                    break;
                case AccountChangedEventTypeContract.Deleted:
                    await HandleDeleted(e);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task HandleDeleted(AccountChangedEvent e)
        {
            var accountId = e.Account.Id;
            _logger.LogInformation("Cancelling all alerts for deleted account {AccountId}",
                accountId);

            _observer.OnNext(new CancelPriceAlertsCommand(OriginalEventType.AccountDeleted)
            {
                AccountId = accountId,
            });
        }

        private async Task HandleUpdated(AccountChangedEvent e)
        {
            if (e.Source != "UpdateClientTradingCondition")
            {
                _logger.LogInformation("Ignoring account update for accountId {AccountId} with source {Source}",
                    e.Account.Id,
                    e.Source);
                return;
            }

            var tradingConditionId = e.Account.TradingConditionId;
            var accountId = e.Account.Id;

            var alerts = (await _priceAlertsCache.GetAllActiveAlerts())
                .Where(x => x.AccountId == accountId);
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

            _observer.OnNext(new CancelPriceAlertsCommand(OriginalEventType.TradingConditionChanged)
            {
                AccountId = accountId,
                Products = unavailableProductsResponse.UnavailableProductIds.ToList(),
            });
        }
    }
}