using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Services;
using MarginTrading.Backend.Contracts;
using MarginTrading.Backend.Contracts.Prices;
using MarginTrading.Backend.Contracts.Snow.Prices;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.DomainServices.Caches
{
    public class QuoteCache : IQuoteCache
    {
        private readonly IPricesApi _pricesApi;
        private readonly IObserver<PriceChangedEvent> _observer;
        private readonly IMapper _mapper;
        private readonly ILogger<QuoteCache> _logger;

        private ConcurrentDictionary<string, QuoteCacheModel> _quotes;

        public QuoteCache(IPricesApi pricesApi,
            IObserver<PriceChangedEvent> observer,
            IMapper mapper,
            ILogger<QuoteCache> logger)
        {
            _pricesApi = pricesApi;
            _observer = observer;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Init()
        {
            var prices = await _pricesApi.GetBestAsync(new InitPricesBackendRequest());
            _quotes = new ConcurrentDictionary<string, QuoteCacheModel>(
                prices.Values.Select(x =>
                    new KeyValuePair<string, QuoteCacheModel>(x.Id, _mapper.Map<BestPriceContract, QuoteCacheModel>(x)))
            );

            _logger.LogInformation("Quote cache initialized: {Count} quotes found", _quotes.Count);
        }

        public Task AddOrUpdate(QuoteCacheModel quote)
        {
            var isSuccess = _quotes.TryGetValue(quote.ProductId, out var previousQuote);
            if (!isSuccess)
            {
                _logger.LogWarning("Previous quote not found for productId {ProductId}", quote.ProductId);
                _quotes.AddOrUpdate(quote.ProductId, quote, (key, oldValue) => quote);
                return Task.CompletedTask;
            }

            if (previousQuote.Timestamp > quote.Timestamp)
            {
                _logger.LogWarning(
                    "Received quote is older than the quote in cache: cached {Cached}, received {Received}",
                    previousQuote.ToJson(),
                    quote.ToJson());
                return Task.CompletedTask;
            }

            _observer.OnNext(new PriceChangedEvent()
            {
                ProductId = quote.ProductId,
                PreviousQuote = new Quote(previousQuote.Ask, previousQuote.Bid),
                CurrentQuote = new Quote(quote.Ask, quote.Bid),
            });

            _quotes.AddOrUpdate(quote.ProductId, quote, (key, oldValue) => quote);

            return Task.CompletedTask;
        }
    }
}