namespace Lykke.Snow.PriceAlerts.Domain.Models
{
    public class PriceChangedEvent
    {
        public string ProductId { get; set; }

        public Quote PreviousQuote { get; set; }
        public Quote CurrentQuote { get; set; }
    }
}