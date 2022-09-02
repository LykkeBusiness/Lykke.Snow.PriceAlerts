using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;
using Lykke.Snow.Common.Startup.ApiKey;

namespace Lykke.Snow.PriceAlerts.Settings
{
    public class PriceAlertsSettings
    {
        public DbSettings Db { get; set; }

        [Optional] public string[] CorsOrigins { get; set; }

        public CqrsSettings Cqrs { get; set; }

        public ServiceSettings AssetService { get; set; }

        public ServiceSettings TradingCore { get; set; }

        [Optional, CanBeNull] public ClientSettings PriceAlertsClient { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }
        
        public ServiceSettings MeteorService { get; set; }
        
        public string ApiAuthority { get; set; }
        public bool RequireHttps { get; set; }
        public bool ValidateIssuerName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientScope { get; set; }
        public int RenewTokenTimeoutSec { get; set; }
    }
}