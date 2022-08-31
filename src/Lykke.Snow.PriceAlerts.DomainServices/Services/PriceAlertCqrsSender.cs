using System.Threading.Tasks;
using AutoMapper;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;
using Lykke.Snow.PriceAlerts.Contract.Models.Events;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Services;

namespace Lykke.Snow.PriceAlerts.DomainServices.Services
{
    public class PriceAlertCqrsSender : IPriceAlertCqrsSender
    {
        private readonly ICqrsMessageSender _messageSender;
        private readonly IProductsCache _productsCache;
        private readonly IMapper _mapper;

        public PriceAlertCqrsSender(ICqrsMessageSender messageSender,
            IProductsCache productsCache,
            IMapper mapper) 
        {
            _messageSender = messageSender;
            _productsCache = productsCache;
            _mapper = mapper;
        }

        public async Task SendPriceAlertTriggeredEvent(PriceAlert alert)
        {
            var product = _productsCache.Get(alert.ProductId);
            await _messageSender.SendEvent(new PriceAlertTriggeredEvent()
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
}