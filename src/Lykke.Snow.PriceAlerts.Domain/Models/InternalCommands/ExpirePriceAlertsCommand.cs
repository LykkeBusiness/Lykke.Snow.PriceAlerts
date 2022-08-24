using System;

namespace Lykke.Snow.PriceAlerts.Domain.Models.InternalCommands
{
    public class ExpirePriceAlertsCommand
    {
        public DateTime ExpirationDate { get; set; }
    }
}