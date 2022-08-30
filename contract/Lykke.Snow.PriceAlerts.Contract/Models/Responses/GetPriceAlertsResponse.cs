using System.Collections.Generic;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Responses
{
    public class GetPriceAlertsResponse : PaginatedResponseContract<PriceAlertContract>
    {
        public GetPriceAlertsResponse(IReadOnlyList<PriceAlertContract> contents, int start, int size, int totalSize) :
            base(contents, start, size, totalSize)
        {
        }
    }
}