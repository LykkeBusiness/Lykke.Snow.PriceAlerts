using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;
using Lykke.Snow.PriceAlerts.Contract.Models.Requests;
using Lykke.Snow.PriceAlerts.Contract.Models.Responses;
using Refit;

namespace Lykke.Snow.PriceAlerts.Contract.Api
{
    [PublicAPI]
    public interface IPriceAlertsApi
    {
        [Get("/api/pricealerts/{id}")]
        Task<GetPriceAlertByIdResponse> GetByIdAsync([Required] [NotNull] string id);

        [Post("/api/pricealerts")]
        Task<ErrorCodeResponse<PriceAlertErrorCodesContract>> AddPriceAlertAsync(
            [Body] AddPriceAlertRequest request);

        [Post("/api/pricealerts/{id}")]
        Task<ErrorCodeResponse<PriceAlertErrorCodesContract>> UpdateAsync(
            [Required] [NotNull] string id, [Body] UpdatePriceAlertRequest request);

        [Delete("/api/pricealerts/{id}")]
        Task<ErrorCodeResponse<PriceAlertErrorCodesContract>> CancelAsync(
            [Required] [NotNull] string id);

        [Get("/api/pricealerts/by-account-id/{accountId}")]
        Task<GetPriceAlertsResponse> GetByAccountIdAsync([Required] [NotNull] string accountId,
            [Body] GetPriceAlertsRequest request);
    }
}