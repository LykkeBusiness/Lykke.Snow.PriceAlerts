using System.Collections.Generic;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Responses
{
    public class GetActivePriceAlertsCountResponse
    {
        public Dictionary<string, int> ActivePriceAlertsByProduct { get; set; }
    }
}