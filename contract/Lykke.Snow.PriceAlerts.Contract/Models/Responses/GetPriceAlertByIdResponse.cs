using Lykke.Snow.Contracts.Responses;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Responses
{
    public class GetPriceAlertByIdResponse : ErrorCodeResponse<PriceAlertErrorCodesContract>
    {
        public PriceAlertContract PriceAlert { get; set; }
    }
}