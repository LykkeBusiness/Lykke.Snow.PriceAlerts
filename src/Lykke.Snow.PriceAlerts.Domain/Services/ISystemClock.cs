using System;

namespace Lykke.Snow.PriceAlerts.Domain.Services
{
    public interface ISystemClock
    {
        DateTime UtcNow();
    }
}