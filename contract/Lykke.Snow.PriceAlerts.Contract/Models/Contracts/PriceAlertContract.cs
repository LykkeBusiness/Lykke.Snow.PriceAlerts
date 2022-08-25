using System;
using MessagePack;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Contracts
{
    [MessagePackObject]
    public class PriceAlertContract
    {
        [Key(0)]
        public string Id { get; set; }

        [Key(1)]
        public string ProductId { get; set; }

        [Key(2)]
        public string AccountId { get; set; }

        [Key(3)]
        public decimal Price { get; set; }
        
        [Key(4)]
        public PriceTypeContract PriceType { get; set; }

        [Key(5)]
        public CrossingDirectionContract Direction { get; set; }

        [Key(6)]
        public AlertStatusContract Status { get; set; }

        /// <summary>
        ///     If validity == null it's GTC (good 'til cancelled)
        /// </summary>
        [Key(7)]
        public DateTime? Validity { get; set; }

        [Key(8)]
        public string Comment { get; set; }

        [Key(9)]
        public DateTime CreatedOn { get; set; }

        [Key(10)]
        public DateTime ModifiedOn { get; set; }
    }
}