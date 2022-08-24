using System;
using System.Collections.Generic;

namespace Lykke.Snow.PriceAlerts.Domain.Models.InternalCommands
{
    public class CancelPriceAlertsCommand
    {
        public CancelPriceAlertsCommand(OriginalEventType type)
        {
            if (type == OriginalEventType.None) throw new ArgumentException("Must provide valid OriginalEventType");
            Type = type;
        }
        
        public List<string> Products { get; set; }

        public string AccountId { get; set; }

        public OriginalEventType Type { get; }
    }
}