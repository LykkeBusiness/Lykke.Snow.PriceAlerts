using Lykke.RabbitMqBroker;
using Lykke.Snow.PriceAlerts.Helpers;
using Lykke.Snow.PriceAlerts.Settings;

namespace Lykke.Snow.PriceAlerts.Extensions
{
    public static class RabbitExtensions
    {
        public static RabbitMqSubscriptionSettings ToInstanceSubscriptionSettings(
            this RabbitConnectionSettings config,
            string instanceId,
            bool isDurable)
        {
            return new RabbitMqSubscriptionSettings
            {
                ConnectionString = config.ConnectionString,
                QueueName = QueueHelper.BuildQueueName(config.ExchangeName, env: instanceId),
                ExchangeName = config.ExchangeName,
                IsDurable = isDurable,
            };
        }
    }
}