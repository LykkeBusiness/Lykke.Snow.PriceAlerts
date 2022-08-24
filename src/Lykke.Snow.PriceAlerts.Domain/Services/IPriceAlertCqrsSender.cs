using System.Threading.Tasks;
using Lykke.Snow.PriceAlerts.Domain.Models;

namespace Lykke.Snow.PriceAlerts.Domain.Services
{
    public interface IPriceAlertCqrsSender
    {
        Task SendPriceAlertTriggeredEvent(PriceAlert alert);
    }
}