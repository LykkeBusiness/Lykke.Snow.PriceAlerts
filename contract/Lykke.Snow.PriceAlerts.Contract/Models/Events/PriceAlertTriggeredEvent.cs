using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Events
{
    public class PriceAlertTriggeredEvent
    {
        public string AlertId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string TradingCurrency { get; set; }
        public decimal Price { get; set; }
        public PriceTypeContract PriceType { get; set; }
        
    }
}