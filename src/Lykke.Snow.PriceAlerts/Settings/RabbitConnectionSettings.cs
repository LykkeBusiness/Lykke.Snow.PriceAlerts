using Lykke.SettingsReader.Attributes;

namespace Lykke.Snow.PriceAlerts.Settings
{
    public class RabbitConnectionSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        
        public string ExchangeName { get; set; }
        
        [Optional]
        public string RoutingKey { get; set; }

        [Optional] 
        public int ConsumerCount { get; set; } = 1;
    }
}