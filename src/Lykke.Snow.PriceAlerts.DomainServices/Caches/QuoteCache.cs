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
        private readonly IMapper _mapper;
        private readonly ILogger<QuoteCache> _logger;

        private ConcurrentDictionary<string, QuoteCacheModel> _quotes;

        public QuoteCache(IPricesApi pricesApi,
            IMapper mapper,
            ILogger<QuoteCache> logger)
        {
            _pricesApi = pricesApi;
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
            _logger.LogInformation("New quote: {Q}", quote.ToJson());
            return Task.CompletedTask;
        }
    }
}