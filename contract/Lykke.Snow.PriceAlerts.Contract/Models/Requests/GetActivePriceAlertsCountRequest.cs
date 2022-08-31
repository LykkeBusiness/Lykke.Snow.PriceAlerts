using System.Collections.Generic;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Requests
{
    public class GetActivePriceAlertsCountRequest
    {
        [Refit.Query(Refit.CollectionFormat.Multi)]
        public List<string> Products { get; set; }

        public string AccountId { get; set; }
    }
}