using System;
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

        // to be moved 1 level up
        public ServiceSettings AssetService { get; set; }

        // to be moved 1 level up
        public ServiceSettings TradingCore { get; set; }

        // to be moved 1 level up
        [Optional, CanBeNull] public ClientSettings PriceAlertsClient { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }

        // to be moved 1 level up
        public ServiceSettings MeteorService { get; set; }

        [Optional] public TimeSpan MeteorMessageExpiration { get; set; } = TimeSpan.FromDays(1);
        
        public OidcSettings OidcSettings { get; set; }
    }
}