using System;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Requests
{
    public class AddPriceAlertRequest
    {
        public string ProductId { get; set; }

        public string AccountId { get; set; }

        public decimal Price { get; set; }

        public PriceTypeContract PriceType { get; set; }

        public CrossingDirectionContract Direction { get; set; }

        /// <summary>
        ///     If validity == null it's GTC (good 'til cancelled)
        /// </summary>
        public DateTime? Validity { get; set; }

        public string Comment { get; set; }
    }
}