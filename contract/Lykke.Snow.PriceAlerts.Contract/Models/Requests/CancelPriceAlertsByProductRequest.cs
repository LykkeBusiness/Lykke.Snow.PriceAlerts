namespace Lykke.Snow.PriceAlerts.Contract.Models.Requests
{
    public class CancelPriceAlertsByProductRequest
    {
        public string AccountId { get; set; }

        public string ProductId { get; set; }
    }
}