using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Requests
{
    public class GetPriceAlertsRequest
    {
        public string ProductId { get; set; }

        [CanBeNull]
        [Refit.Query(Refit.CollectionFormat.Multi)]
        public List<AlertStatusContract> Statuses { get; set; }

        public int? Skip { get; set; }

        public int? Take { get; set; }
    }
}