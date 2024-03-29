using System;
using Lykke.Snow.PriceAlerts.Domain.Models;

namespace Lykke.Snow.PriceAlerts.SqlRepositories.Entities
{
    public class PriceAlertEntity
    {
        public string Id { get; set; }

        public string ProductId { get; set; }

        public string AccountId { get; set; }

        public decimal Price { get; set; }

        public PriceType PriceType { get; set; }

        public CrossingDirection Direction { get; set; }

        public AlertStatus Status { get; set; }

        /// <summary>
        ///     If validity == null it's GTC (good 'til cancelled)
        /// </summary>
        public DateTime? Validity { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        /// <summary>
        ///     CorrelationId of the quote that triggered the alert
        /// </summary>
        public string CorrelationId { get; set; }
    }
}