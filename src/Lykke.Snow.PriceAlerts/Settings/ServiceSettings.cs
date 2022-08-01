using Lykke.SettingsReader.Attributes;

namespace Lykke.Snow.PriceAlerts.Settings
{
    public class ServiceSettings
    {
        [HttpCheck("/api/isalive")] public string Url { get; set; }

        [Optional] public string ApiKey { get; set; }
    }
}