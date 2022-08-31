using System.Threading.Tasks;
using Lykke.Snow.Contracts.Messaging;

namespace Lykke.Snow.PriceAlerts.Domain.Services
{
    public interface ICqrsEntityChangedSender
    {
        Task SendEntityCreatedEvent<TModel, TContract, TEvent>(TModel newValue, string username = null)
            where TEvent : EntityChangedEvent<TContract>, new()
            where TModel : class;

        Task SendEntityEditedEvent<TModel, TContract, TEvent>(TModel oldValue, TModel newValue, string username = null)
            where TEvent : EntityChangedEvent<TContract>, new()
            where TModel : class;

        Task SendEntityDeletedEvent<TModel, TContract, TEvent>(TModel oldValue, string username = null)
            where TEvent : EntityChangedEvent<TContract>, new()
            where TModel : class;
    }
}