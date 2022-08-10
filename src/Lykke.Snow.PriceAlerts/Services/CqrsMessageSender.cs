using System;
using System.Threading.Tasks;
using Lykke.Cqrs;
using Lykke.Snow.PriceAlerts.Domain.Services;
using Lykke.Snow.PriceAlerts.Settings;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.Services
{
    public class CqrsMessageSender : ICqrsMessageSender
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ILogger<CqrsMessageSender> _logger;
        private readonly CqrsContextNamesSettings _contextNames;

        public CqrsMessageSender(
            ICqrsEngine cqrsEngine,
            ILogger<CqrsMessageSender> logger,
            CqrsContextNamesSettings contextNames)
        {
            _cqrsEngine = cqrsEngine;
            _logger = logger;
            _contextNames = contextNames;
        }

        public void SendEvent<TEvent>(TEvent @event)
        {
            try
            {
                _cqrsEngine.PublishEvent(@event, _contextNames.PriceAlertsService);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error on sending event: {Message}", ex.Message);
            }
        }
    }
}