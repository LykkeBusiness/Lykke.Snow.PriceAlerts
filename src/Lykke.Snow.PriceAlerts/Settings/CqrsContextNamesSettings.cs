namespace Lykke.Snow.PriceAlerts.Settings
{
    public class CqrsContextNamesSettings
    {
        public string PriceAlertsService { get; set; } = nameof(PriceAlertsService);
        public string SettingsService { get; set; } = nameof(SettingsService);
        public string TradingEngine { get; set; } = nameof(TradingEngine);
        public string AccountsManagement { get; set; } = nameof(AccountsManagement);
    }
}