using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Snow.PriceAlerts.Domain.Models.InternalCommands;
using MarginTrading.Backend.Contracts.TradingSchedule;

namespace Lykke.Snow.PriceAlerts.Projections
{
    public class ExpirationProcessProjection
    {
        private readonly IObserver<ExpirePriceAlertsCommand> _observer;

        public ExpirationProcessProjection(IObserver<ExpirePriceAlertsCommand> observer)
        {
            _observer = observer;
        }

        [UsedImplicitly]
        public Task Handle(ExpirationProcessStartedEvent @event)
        {
            _observer.OnNext(new ExpirePriceAlertsCommand()
            {
                ExpirationDate = @event.OperationIntervalEnd,
            });

            return Task.CompletedTask;
        }
    }
}