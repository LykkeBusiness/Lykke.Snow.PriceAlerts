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

        [Optional, CanBeNull]
        public ClientSettings PriceAlertsClient { get; set; }
    }
}