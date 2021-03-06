using Lykke.SettingsReader.Attributes;

namespace Lykke.Snow.PriceAlerts.Settings
{
    public class PriceAlertsSettings
    {
        public DbSettings Db { get; set; }

        [Optional]
        public string[] CorsOrigins { get; set; }
    }
}