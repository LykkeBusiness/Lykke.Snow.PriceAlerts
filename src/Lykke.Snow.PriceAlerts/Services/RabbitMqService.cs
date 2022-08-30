using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Publisher.Serializers;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.RabbitMqBroker.Subscriber.Deserializers;
using Lykke.RabbitMqBroker.Subscriber.Middleware.ErrorHandling;
using Lykke.Snow.Common.Correlation.RabbitMq;
using Lykke.Snow.PriceAlerts.Helpers;
using Lykke.Snow.PriceAlerts.Settings;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.Services
{
    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly ILogger<RabbitMqService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly RabbitMqCorrelationManager _correlationManager;

        private readonly ConcurrentDictionary<(RabbitMqSubscriptionSettings, int), IStartStop> _subscribers =
            new ConcurrentDictionary<(RabbitMqSubscriptionSettings, int), IStartStop>(
                new SubscriptionSettingsWithNumberEqualityComparer());

        private readonly ConcurrentDictionary<RabbitMqSubscriptionSettings, IStartStop> _producers =
            new ConcurrentDictionary<RabbitMqSubscriptionSettings, IStartStop>(
                new SubscriptionSettingsEqualityComparer());

        public RabbitMqService(ILogger<RabbitMqService> logger,
            ILoggerFactory loggerFactory,
            RabbitMqCorrelationManager correlationManager)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _correlationManager = correlationManager;
        }

        public void Dispose()
        {
            foreach (var stoppable in _subscribers.Values)
                stoppable.Stop();
            foreach (var stoppable in _producers.Values)
                stoppable.Stop();
        }

        public IRabbitMqSerializer<TMessage> GetJsonSerializer<TMessage>()
        {
            return new JsonMessageSerializer<TMessage>();
        }

        public IRabbitMqSerializer<TMessage> GetMsgPackSerializer<TMessage>()
        {
            return new MessagePackMessageSerializer<TMessage>();
        }

        public IMessageDeserializer<TMessage> GetJsonDeserializer<TMessage>()
        {
            return new DeserializerWithErrorLogging<TMessage>(
                _loggerFactory.CreateLogger<DeserializerWithErrorLogging<TMessage>>()
            );
        }

        public IMessageDeserializer<TMessage> GetMsgPackDeserializer<TMessage>()
        {
            return new MessagePackMessageDeserializer<TMessage>();
        }

        public IMessageProducer<TMessage> GetProducer<TMessage>(RabbitConnectionSettings settings,
            bool isDurable, IRabbitMqSerializer<TMessage> serializer)
        {
            // on-the fly connection strings switch is not supported currently for rabbitMq
            var subscriptionSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = settings.ConnectionString,
                ExchangeName = settings.ExchangeName,
                RoutingKey = settings.RoutingKey ?? string.Empty,
                IsDurable = isDurable,
            };

            return (IMessageProducer<TMessage>) _producers.GetOrAdd(subscriptionSettings, CreateProducer);

            IStartStop CreateProducer(RabbitMqSubscriptionSettings s)
            {
                var publisher = new RabbitMqPublisher<TMessage>(_loggerFactory, s);

                publisher.DisableInMemoryQueuePersistence();

                publisher = publisher
                    .SetSerializer(serializer)
                    .SetWriteHeadersFunc(_correlationManager.BuildCorrelationHeadersIfExists);
                publisher.Start();
                return publisher;
            }
        }

        public void Subscribe<TMessage>(RabbitConnectionSettings settings, bool isDurable,
            Func<TMessage, Task> handler, IMessageDeserializer<TMessage> deserializer,
            string instanceId = null)
        {
            var consumerCount = settings.ConsumerCount <= 0 ? 1 : settings.ConsumerCount;

            foreach (var consumerNumber in Enumerable.Range(1, consumerCount))
            {
                var subscriptionSettings = new RabbitMqSubscriptionSettings
                {
                    ConnectionString = settings.ConnectionString,
                    QueueName = QueueHelper.BuildQueueName(settings.ExchangeName, instanceId),
                    ExchangeName = settings.ExchangeName,
                    IsDurable = isDurable,
                };

                var rabbitMqSubscriber = new RabbitMqSubscriber<TMessage>(
                        _loggerFactory.CreateLogger<RabbitMqSubscriber<TMessage>>(),
                        subscriptionSettings)
                    .SetMessageDeserializer(deserializer)
                    .Subscribe(handler)
                    .UseMiddleware(new ExceptionSwallowMiddleware<TMessage>(
                        _loggerFactory.CreateLogger<ExceptionSwallowMiddleware<TMessage>>()))
                    .SetReadHeadersAction(_correlationManager.FetchCorrelationIfExists);

                if (!_subscribers.TryAdd((subscriptionSettings, consumerNumber), rabbitMqSubscriber))
                {
                    throw new InvalidOperationException(
                        $"Subscriber #{consumerNumber} for queue {subscriptionSettings.QueueName} was already initialized");
                }

                rabbitMqSubscriber.Start();
            }
        }

        /// <remarks>
        ///     ReSharper auto-generated
        /// </remarks>
        private sealed class SubscriptionSettingsEqualityComparer : IEqualityComparer<RabbitMqSubscriptionSettings>
        {
            public bool Equals(RabbitMqSubscriptionSettings x, RabbitMqSubscriptionSettings y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.ConnectionString, y.ConnectionString) &&
                       string.Equals(x.ExchangeName, y.ExchangeName);
            }

            public int GetHashCode(RabbitMqSubscriptionSettings obj)
            {
                unchecked
                {
                    return ((obj.ConnectionString != null ? obj.ConnectionString.GetHashCode() : 0) * 397) ^
                           (obj.ExchangeName != null ? obj.ExchangeName.GetHashCode() : 0);
                }
            }
        }

        /// <remarks>
        ///     ReSharper auto-generated
        /// </remarks>
        private sealed class
            SubscriptionSettingsWithNumberEqualityComparer : IEqualityComparer<(RabbitMqSubscriptionSettings, int)>
        {
            public bool Equals((RabbitMqSubscriptionSettings, int) x, (RabbitMqSubscriptionSettings, int) y)
            {
                if (ReferenceEquals(x.Item1, y.Item1) && x.Item2 == y.Item2) return true;
                if (ReferenceEquals(x.Item1, null)) return false;
                if (ReferenceEquals(y.Item1, null)) return false;
                if (x.Item1.GetType() != y.Item1.GetType()) return false;
                return string.Equals(x.Item1.ConnectionString, y.Item1.ConnectionString)
                       && string.Equals(x.Item1.ExchangeName, y.Item1.ExchangeName)
                       && x.Item2 == y.Item2;
            }

            public int GetHashCode((RabbitMqSubscriptionSettings, int) obj)
            {
                unchecked
                {
                    return ((obj.Item1.ConnectionString != null ? obj.Item1.ConnectionString.GetHashCode() : 0) * 397) ^
                           (obj.Item1.ExchangeName != null ? obj.Item1.ExchangeName.GetHashCode() : 0);
                }
            }
        }
    }
}