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

        public ServiceSettings AssetService { get; set; }

        public ServiceSettings TradingCore { get; set; }

        [Optional, CanBeNull] public ClientSettings PriceAlertsClient { get; set; }

        public RabbitMqSettings RabbitMq { get; set; }

        public ServiceSettings MeteorService { get; set; }

        public OidcSettings OidcSettings { get; set; }

        public TimeSpan MeteorMessageExpiration { get; set; } = TimeSpan.FromDays(1);
    }
}