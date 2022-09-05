using System.Threading.Tasks;
using Lykke.Snow.PriceAlerts.Domain.Models;

namespace Lykke.Snow.PriceAlerts.Domain.Services
{
    public interface IMeteorSender
    {
        Task SendPriceAlertTriggered(PriceAlert priceAlert);
    }
}