namespace Lykke.Snow.PriceAlerts.Settings
{
    public class OidcSettings
    {
        public string ApiAuthority { get; set; }

        public bool ValidateIssuerName { get; set; }

        public bool RequireHttps { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string ClientScope { get; set; }

        public int RenewTokenTimeoutSec { get; set; }
    }
}