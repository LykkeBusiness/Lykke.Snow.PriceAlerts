namespace Lykke.Snow.PriceAlerts.Domain.Services
{
    public interface ICqrsMessageSender
    {
        void SendEvent<TEvent>(TEvent @event);
    }
}