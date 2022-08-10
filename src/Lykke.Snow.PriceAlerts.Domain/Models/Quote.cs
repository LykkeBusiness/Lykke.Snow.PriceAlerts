namespace Lykke.Snow.PriceAlerts.Domain.Models
{
    public class Quote
    {
        public Quote(decimal ask, decimal bid)
        {
            Ask = ask;
            Bid = bid;
        }

        public decimal Bid { get; }
        public decimal Ask { get; }
        public decimal Mid => (Bid + Ask) / 2;
    }
}