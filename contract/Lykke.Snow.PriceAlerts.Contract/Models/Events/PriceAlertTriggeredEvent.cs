using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;
using MessagePack;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Events
{
    [MessagePackObject]
    public class PriceAlertTriggeredEvent
    {
        [Key(0)]
        public string AlertId { get; set; }
        [Key(1)]
        public string AccountId { get; set; }
        [Key(2)]
        public string ProductId { get; set; }
        [Key(3)]
        public string ProductName { get; set; }
        [Key(4)]
        public string TradingCurrency { get; set; }
        [Key(5)]
        public decimal Price { get; set; }
        [Key(6)]
        public PriceTypeContract PriceType { get; set; }
    }
}