using System;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Contracts
{
    public class PriceAlertContract
    {
        public string Id { get; set; }

        public string ProductId { get; set; }

        public string AccountId { get; set; }

        public decimal Price { get; set; }

        public PriceTypeContract PriceType { get; set; }

        public CrossingDirectionContract Direction { get; set; }

        public AlertStatusContract Status { get; set; }

        /// <summary>
        ///     If validity == null it's GTC (good 'til cancelled)
        /// </summary>
        public DateTime? Validity { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}