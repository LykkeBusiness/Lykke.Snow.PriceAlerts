using System;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Requests
{
    public class UpdatePriceAlertRequest
    {
        public string AccountId { get; set; }

        public decimal Price { get; set; }

        /// <summary>
        ///     If validity == null it's GTC (good 'til cancelled)
        /// </summary>
        public DateTime? Validity { get; set; }

        public string Comment { get; set; }
    }
}