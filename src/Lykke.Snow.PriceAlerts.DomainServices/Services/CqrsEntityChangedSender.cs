using System;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Snow.Common.Correlation;
using Lykke.Snow.Contracts.Messaging;
using Lykke.Snow.PriceAlerts.Domain.Services;

namespace Lykke.Snow.PriceAlerts.DomainServices.Services
{
    public class CqrsEntityChangedSender : ICqrsEntityChangedSender
    {
        private readonly ICqrsMessageSender _messageSender;
        private readonly IMapper _mapper;
        private readonly CorrelationContextAccessor _correlationContextAccessor;

        public CqrsEntityChangedSender(ICqrsMessageSender messageSender,
            IMapper mapper,
            CorrelationContextAccessor correlationContextAccessor)
        {
            _messageSender = messageSender;
            _mapper = mapper;
            _correlationContextAccessor = correlationContextAccessor;
        }

        public Task SendEntityCreatedEvent<TModel, TContract, TEvent>(TModel newValue, string username = null)
            where TEvent : EntityChangedEvent<TContract>, new()
            where TModel : class
            => SendEntityChangedEvent<TModel, TContract, TEvent>(null, newValue, username, ChangeType.Creation);

        public Task SendEntityEditedEvent<TModel, TContract, TEvent>(TModel oldValue, TModel newValue,
            string username = null)
            where TEvent : EntityChangedEvent<TContract>, new()
            where TModel : class
            => SendEntityChangedEvent<TModel, TContract, TEvent>(oldValue, newValue, username, ChangeType.Edition);

        public Task SendEntityDeletedEvent<TModel, TContract, TEvent>(TModel oldValue, string username = null)
            where TEvent : EntityChangedEvent<TContract>, new()
            where TModel : class
            => SendEntityChangedEvent<TModel, TContract, TEvent>(oldValue, null, username, ChangeType.Deletion);

        public Task SendEntityCreatedEvent<TModel, TContract, TContext, TContextContract, TEvent>(TModel newValue,
            TContext context,
            string username = null) where TModel : class
            where TEvent : EntityChangedEvent<TContract, TContextContract>, new()
            where TContext : class
            where TContextContract : class
            => SendEntityChangedEvent<TModel, TContract, TContext, TContextContract, TEvent>(null, newValue, context,
                username, ChangeType.Creation);

        public Task SendEntityEditedEvent<TModel, TContract, TContext, TContextContract, TEvent>(TModel oldValue,
            TModel newValue,
            TContext context, string username = null) where TModel : class
            where TEvent : EntityChangedEvent<TContract, TContextContract>, new()
            where TContext : class
            where TContextContract : class
            => SendEntityChangedEvent<TModel, TContract, TContext, TContextContract, TEvent>(oldValue, newValue,
                context, username, ChangeType.Edition);

        public Task SendEntityDeletedEvent<TModel, TContract, TContext, TContextContract, TEvent>(TModel oldValue,
            TContext context,
            string username = null) where TModel : class
            where TEvent : EntityChangedEvent<TContract, TContextContract>, new()
            where TContext : class
            where TContextContract : class
            => SendEntityChangedEvent<TModel, TContract, TContext, TContextContract, TEvent>(oldValue, null, context,
                username, ChangeType.Deletion);


        private async Task SendEntityChangedEvent<TModel, TContract, TEvent>(TModel oldValue, TModel newValue,
            string username, ChangeType changeType)
            where TEvent : EntityChangedEvent<TContract>, new()
            where TModel : class
        {
            await _messageSender.SendEvent(new TEvent()
            {
                Username = username,
                ChangeType = changeType,
                CorrelationId = _correlationContextAccessor.GetOrGenerateCorrelationId(),
                EventId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                OldValue = _mapper.Map<TModel, TContract>(oldValue),
                NewValue = _mapper.Map<TModel, TContract>(newValue),
            });
        }

        private async Task SendEntityChangedEvent<TModel, TContract, TContext, TContextContract, TEvent>(
            TModel oldValue,
            TModel newValue,
            TContext context,
            string username,
            ChangeType changeType)
            where TEvent : EntityChangedEvent<TContract, TContextContract>, new()
            where TModel : class
            where TContext : class
            where TContextContract : class
        {
            await _messageSender.SendEvent(new TEvent()
            {
                Username = username,
                ChangeType = changeType,
                CorrelationId = _correlationContextAccessor.GetOrGenerateCorrelationId(),
                EventId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                OldValue = _mapper.Map<TModel, TContract>(oldValue),
                NewValue = _mapper.Map<TModel, TContract>(newValue),
                Context = _mapper.Map<TContext, TContextContract>(context),
            });
        }
    }
}