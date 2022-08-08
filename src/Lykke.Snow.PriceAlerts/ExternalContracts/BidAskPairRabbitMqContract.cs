using System;
using Newtonsoft.Json;

namespace Lykke.Snow.PriceAlerts.ExternalContracts
{
    /// <summary>
    /// Contract from MT Core
    /// for some reason it's not declared in the main contracts library of mt core
    /// </summary>
    public class BidAskPairRabbitMqContract
    {
        public string Instrument { get; set; }
        public DateTime Date { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsEod { get; set; }
    }
}