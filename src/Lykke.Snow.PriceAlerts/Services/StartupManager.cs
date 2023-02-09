using System.Threading.Tasks;
using AutoMapper;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Services;
using Lykke.Snow.PriceAlerts.ExternalContracts;
using Lykke.Snow.PriceAlerts.Settings;

namespace Lykke.Snow.PriceAlerts.Services
{
    public class StartupManager
    {
        private readonly IPriceAlertsCache _priceAlertsCache;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly IQuoteCache _quoteCache;
        private readonly IMapper _mapper;
        private readonly PriceAlertsSettings _settings;
        private readonly IProductsCache _productsCache;

        public StartupManager(IProductsCache productsCache,
            IPriceAlertsCache priceAlertsCache,
            IRabbitMqService rabbitMqService,
            IQuoteCache quoteCache,
            IMapper mapper,
            PriceAlertsSettings settings)
        {
            _productsCache = productsCache;
            _priceAlertsCache = priceAlertsCache;
            _rabbitMqService = rabbitMqService;
            _quoteCache = quoteCache;
            _mapper = mapper;
            _settings = settings;
        }

        internal async Task Start()
        {
            await _productsCache.Init();
            await _priceAlertsCache.Init();
            await _quoteCache.Init();

            await StartRabbitMqServices();
        }

        private async Task StartRabbitMqServices()
        {
            _rabbitMqService.Subscribe(_settings.RabbitMq.Consumers.QuotesRabbitMqSettings,
                false,
                quote => _quoteCache.AddOrUpdate(
                    _mapper.Map<BidAskPairRabbitMqContract, QuoteCacheModel>(quote)),
                _rabbitMqService.GetJsonDeserializer<BidAskPairRabbitMqContract>());
        }
    }
}