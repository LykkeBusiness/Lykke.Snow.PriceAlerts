using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Snow.PriceAlerts.Domain.Extensions;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Services;
using Meteor.Client;
using Meteor.Client.Models;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.DomainServices.Services
{
    public class MeteorSender : IMeteorSender
    {
        private readonly IMeteorClient _meteorClient;
        private readonly IProductsCache _productsCache;
        private readonly TimeSpan _expiration;
        private readonly ILogger<MeteorSender> _logger;

        public MeteorSender(IMeteorClient meteorClient,
            IProductsCache productsCache,
            TimeSpan expiration,
            ILogger<MeteorSender> logger)
        {
            _meteorClient = meteorClient;
            _productsCache = productsCache;
            _expiration = expiration;
            _logger = logger;
        }
        
        public async Task SendPriceAlertTriggered(PriceAlert priceAlert)
        {
            var product = _productsCache.Get(priceAlert.ProductId);

            var list = new List<string>
            {
                product.Name,
                priceAlert.Price.ToUiString(), // TODO: get correct precision for product
                product.TradingCurrency,
                priceAlert.PriceType.ToString(),
                priceAlert.Direction.ToString(),
                priceAlert.Comment
            };
                
            var response = await _meteorClient.SendMessage(new SystemMessageRequestModel
            {
                RequiresPopup = true,
                ExpirationTimestamp = DateTime.UtcNow.Add(_expiration).Date,
                Recipients = priceAlert.AccountId,
                Event = MessageEventType.PriceAlertTriggered,
                IsImportant = true,
                LocalizationAttributes = list.ToArray()
            });
                
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Could not send message for price alert triggered. Price alert id: {priceAlert.Id}. Status code: {response.StatusCode}, response content: {content}");
            }
        }
    }
}