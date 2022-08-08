using System;

namespace Lykke.Snow.PriceAlerts.Domain.Models
{
    public class QuoteCacheModel
    {
        public string ProductId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
    }
}