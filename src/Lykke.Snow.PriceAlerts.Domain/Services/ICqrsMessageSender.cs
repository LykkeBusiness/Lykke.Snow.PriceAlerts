using System.Threading.Tasks;

namespace Lykke.Snow.PriceAlerts.Domain.Services
{
    public interface ICqrsMessageSender
    {
        ValueTask SendEvent<TEvent>(TEvent @event);
    }
}