using System.Threading.Tasks;
using Lykke.Contracts.Messaging;

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

        Task SendEntityCreatedEvent<TModel, TContract, TContext, TContextContract, TEvent>(TModel newValue,
            TContext context,
            string username = null)
            where TEvent : EntityChangedEvent<TContract, TContextContract>, new()
            where TModel : class
            where TContext : class
            where TContextContract : class;

        Task SendEntityEditedEvent<TModel, TContract, TContext, TContextContract, TEvent>(TModel oldValue,
            TModel newValue,
            TContext context,
            string username = null)
            where TEvent : EntityChangedEvent<TContract, TContextContract>, new()
            where TModel : class
            where TContext : class
            where TContextContract : class;

        Task SendEntityDeletedEvent<TModel, TContract, TContext, TContextContract, TEvent>(TModel oldValue,
            TContext context,
            string username = null)
            where TEvent : EntityChangedEvent<TContract, TContextContract>, new()
            where TModel : class
            where TContext : class
            where TContextContract : class;
    }
}