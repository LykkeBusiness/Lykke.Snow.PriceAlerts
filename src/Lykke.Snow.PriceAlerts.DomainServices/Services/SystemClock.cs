using System;
using Lykke.Snow.PriceAlerts.Domain.Services;

namespace Lykke.Snow.PriceAlerts.DomainServices.Services
{
    public class SystemClock : ISystemClock
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }
    }
}