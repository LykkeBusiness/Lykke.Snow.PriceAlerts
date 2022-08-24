using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Models.InternalCommands;
using Lykke.Snow.PriceAlerts.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.DomainServices.Services
{
    public class PriceAlertsEngine : IHostedService
    {
        private readonly IPriceAlertsService _priceAlertsService;
        private readonly IPriceAlertsCache _priceAlertsCache;
        private readonly IObservable<PriceChangedEvent> _priceChangedObservable;
        private readonly IObservable<CancelPriceAlertsCommand> _cancelPriceAlertObservable;
        private readonly IObservable<ExpirePriceAlertsCommand> _expirePriceAlertsObservable;
        private readonly ICqrsMessageSender _cqrsMessageSender;
        private readonly IProductsCache _productsCache;
        private readonly IMapper _mapper;
        private readonly ILogger<PriceAlertsEngine> _logger;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        public PriceAlertsEngine(
            IPriceAlertsService priceAlertsService,
            IPriceAlertsCache priceAlertsCache,
            IObservable<PriceChangedEvent> priceChangedObservable,
            IObservable<CancelPriceAlertsCommand> cancelPriceAlertObservable,
            IObservable<ExpirePriceAlertsCommand> expirePriceAlertsObservable,
            ICqrsMessageSender cqrsMessageSender,
            IProductsCache productsCache,
            IMapper mapper,
            ILogger<PriceAlertsEngine> logger)
        {
            _priceAlertsService = priceAlertsService;
            _priceAlertsCache = priceAlertsCache;
            _priceChangedObservable = priceChangedObservable;
            _cancelPriceAlertObservable = cancelPriceAlertObservable;
            _expirePriceAlertsObservable = expirePriceAlertsObservable;
            _cqrsMessageSender = cqrsMessageSender;
            _productsCache = productsCache;
            _mapper = mapper;
            _logger = logger;
        }

        #region IHostedService

        public Task StartAsync(CancellationToken cancellationToken)
        {
            AddSubscription(_priceChangedObservable, OnPriceChange);
            AddSubscription(_cancelPriceAlertObservable, OnCancel);
            AddSubscription(_expirePriceAlertsObservable, OnExpire);

            return Task.CompletedTask;
        }

        private void AddSubscription<T>(IObservable<T> observable, Func<T, Task> handler)
        {
            _subscriptions.Add(observable
                .SelectMany(async x => await Wrap(x, handler))
                .Subscribe()
            );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscriptions.ForEach(x => x.Dispose());
            return Task.CompletedTask;
        }

        private async Task<Unit> Wrap<TEvent>(TEvent @event, Func<TEvent, Task> handler)
        {
            try
            {
                await handler(@event);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            return Unit.Default;
        }

        #endregion

        #region Engine Implementation

        private async Task OnCancel(CancelPriceAlertsCommand command)
        {
            switch (command.Type)
            {
                case OriginalEventType.AccountDeleted:
                    await _priceAlertsService.CancelByProductAndAccountAsync(accountId: command.AccountId);
                    break;

                case OriginalEventType.TradingConditionChanged:
                    foreach (var product in command.Products)
                    {
                        await _priceAlertsService.CancelByProductAndAccountAsync(product, command.AccountId);
                    }

                    break;

                case OriginalEventType.InvalidProduct:
                    foreach (var product in command.Products)
                    {
                        await _priceAlertsService.CancelByProductAndAccountAsync(product);
                    }

                    break;

                case OriginalEventType.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task OnExpire(ExpirePriceAlertsCommand command)
        {
            await _priceAlertsService.ExpireAllAsync(command.ExpirationDate);
        }

        private async Task OnPriceChange(PriceChangedEvent @event)
        {
            var priceAlerts = await _priceAlertsCache.GetAllActiveAlerts();
            priceAlerts = priceAlerts.Where(x => x.ProductId == @event.ProductId);

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
                    var result = await _priceAlertsService.TriggerAsync(alert.Id);
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation(
                            "Alert triggered: alertId {Id}, previousPrice: {PreviousPrice}, currentPrice {CurrentPrice}, alertPrice {Price}",
                            alert.Id,
                            previousPrice,
                            currentPrice,
                            alert.Price);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Alert trigger failed: alertId {Id}, previousPrice: {PreviousPrice}, currentPrice {CurrentPrice}, alertPrice {Price}, error {Error}",
                            alert.Id,
                            previousPrice,
                            currentPrice,
                            alert.Price,
                            result.Error?.ToString());
                    }
                }
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

        #endregion
    }
}