using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;
using Lykke.Snow.PriceAlerts.Contract.Models.Events;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.DomainServices.Services
{
    public class PriceAlertsEngine : IHostedService
    {
        private readonly IPriceAlertsService _priceAlertsService;
        private readonly IObservable<PriceChangedEvent> _observable;
        private readonly ICqrsMessageSender _cqrsMessageSender;
        private readonly IProductsCache _productsCache;
        private readonly IMapper _mapper;
        private readonly ILogger<PriceAlertsEngine> _logger;
        private IDisposable _subscription;

        public PriceAlertsEngine(
            IPriceAlertsService priceAlertsService,
            IObservable<PriceChangedEvent> observable,
            ICqrsMessageSender cqrsMessageSender,
            IProductsCache productsCache,
            IMapper mapper,
            ILogger<PriceAlertsEngine> logger)
        {
            _priceAlertsService = priceAlertsService;
            _observable = observable;
            _cqrsMessageSender = cqrsMessageSender;
            _productsCache = productsCache;
            _mapper = mapper;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscription = _observable
                .SelectMany(async x => await OnPriceChangeWrapper(x))
                .Subscribe();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription?.Dispose();
            return Task.CompletedTask;
        }

        private async Task<Unit> OnPriceChangeWrapper(PriceChangedEvent @event)
        {
            try
            {
                await OnPriceChange(@event);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return Unit.Default;
        }

        private async Task OnPriceChange(PriceChangedEvent @event)
        {
            var priceAlerts = await _priceAlertsService.GetActiveByProductIdAsync(@event.ProductId);

            var grouped = priceAlerts.GroupBy(x => x.PriceType);
            foreach (var group in grouped)
            {
                var priceType = group.Key;
                var (previousPrice, currentPrice) = GetPriceByType(priceType, @event);
                var crossingDirection = currentPrice > previousPrice ? CrossingDirection.Up : CrossingDirection.Down;
                Func<decimal, decimal, decimal, bool> predicate = GetPricePredicate(crossingDirection);

                var alerts = group
                    .Where(x => x.Direction == crossingDirection)
                    .Where(x => predicate(x.Price, previousPrice, currentPrice));

                foreach (var alert in alerts)
                {
                    _logger.LogInformation(
                        "Alert triggered: alertId {Id}, previousPrice: {PreviousPrice}, currentPrice {CurrentPrice}, alertPrice {Price}",
                        alert.Id,
                        previousPrice,
                        currentPrice,
                        alert.Price);

                    await Trigger(alert);
                }
            }
        }

        private async Task Trigger(PriceAlert alert)
        {
            var result = await _priceAlertsService.TriggerAsync(alert.Id);
            if (result.IsSuccess)
            {
                var product = _productsCache.Get(alert.ProductId);
                _cqrsMessageSender.SendEvent(new PriceAlertTriggeredEvent()
                {
                    Price = alert.Price,
                    AlertId = alert.Id,
                    AccountId = alert.AccountId,
                    PriceType = _mapper.Map<PriceType, PriceTypeContract>(alert.PriceType),
                    ProductId = alert.ProductId,
                    ProductName = product.Name,
                    TradingCurrency = product.TradingCurrency,
                });
            }
        }

        private Func<decimal, decimal, decimal, bool> GetPricePredicate(CrossingDirection crossingDirection)
        {
            return crossingDirection switch
            {
                CrossingDirection.Up => (alertPrice, previousPrice, currentPrice) =>
                    previousPrice < alertPrice && alertPrice < currentPrice,
                CrossingDirection.Down => (alertPrice, previousPrice, currentPrice) =>
                    previousPrice > alertPrice && alertPrice > currentPrice,
                _ => throw new ArgumentOutOfRangeException(nameof(crossingDirection), crossingDirection, null)
            };
        }

        private (decimal previousPrice, decimal currentPrice) GetPriceByType(PriceType type, PriceChangedEvent @event)
        {
            return type switch
            {
                PriceType.Bid => (@event.PreviousQuote.Bid, @event.CurrentQuote.Bid),
                PriceType.Mid => (@event.PreviousQuote.Mid, @event.CurrentQuote.Mid),
                PriceType.Ask => (@event.PreviousQuote.Ask, @event.CurrentQuote.Ask),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Price type not found")
            };
        }
    }
}