using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Requests
{
    public class GetPriceAlertsRequest
    {
        public string ProductId { get; set; }

        public AlertStatusContract? Status { get; set; }

        public int? Skip { get; set; }

        public int? Take { get; set; }
    }
}