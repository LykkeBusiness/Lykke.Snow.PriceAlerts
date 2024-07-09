using System.Threading.Tasks;

using Lykke.Cqrs;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Snow.PriceAlerts.Domain.Services;
using Lykke.Snow.PriceAlerts.ExternalContracts;

namespace Lykke.Snow.PriceAlerts.Services
{
    public class StartupManager
    {
        private readonly IPriceAlertsCache _priceAlertsCache;
        private readonly IQuoteCache _quoteCache;
        private readonly IProductsCache _productsCache;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly RabbitMqListener<BidAskPairRabbitMqContract> _quoteListener;

        public StartupManager(
            IProductsCache productsCache,
            IPriceAlertsCache priceAlertsCache,
            IQuoteCache quoteCache,
            ICqrsEngine cqrsEngine,
            RabbitMqListener<BidAskPairRabbitMqContract> quoteListener)
        {
            _productsCache = productsCache;
            _priceAlertsCache = priceAlertsCache;
            _quoteCache = quoteCache;
            _cqrsEngine = cqrsEngine;
            _quoteListener = quoteListener;
        }

        internal async Task Start()
        {
            await _productsCache.Init();
            await _priceAlertsCache.Init();
            await _quoteCache.Init();

            _quoteListener.Start();

            _cqrsEngine.StartPublishers();
            _cqrsEngine.StartSubscribers();
        }
    }
}