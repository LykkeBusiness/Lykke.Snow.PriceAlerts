using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Snow.PriceAlerts.Domain.Services;
using MarginTrading.Backend.Contracts.TradingSchedule;

namespace Lykke.Snow.PriceAlerts.Projections
{
    public class ExpirationProcessProjection
    {
        private readonly IPriceAlertsService _priceAlertsService;

        public ExpirationProcessProjection(IPriceAlertsService priceAlertsService)
        {
            _priceAlertsService = priceAlertsService;
        }

        [UsedImplicitly]
        public async Task Handle(ExpirationProcessStartedEvent @event)
        {
            await _priceAlertsService.ExpireAllAsync(@event.OperationIntervalEnd);
        }
    }
}