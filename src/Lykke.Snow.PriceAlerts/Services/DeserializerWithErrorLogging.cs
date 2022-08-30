using System;
using System.Text;
using Lykke.RabbitMqBroker.Subscriber.Deserializers;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.Services
{
    public class DeserializerWithErrorLogging<TMessage> : IMessageDeserializer<TMessage>
    {
        private readonly IMessageDeserializer<TMessage> _baseDeserializer;
        private readonly ILogger _logger;

        public DeserializerWithErrorLogging(ILogger<DeserializerWithErrorLogging<TMessage>> logger,
            IMessageDeserializer<TMessage> baseDeserializer = null)
        {
            _logger = logger;
            _baseDeserializer =
                baseDeserializer ?? new JsonMessageDeserializer<TMessage>();
        }

        public TMessage Deserialize(byte[] data)
        {
            try
            {
                return _baseDeserializer.Deserialize(data);
            }
            catch (Exception e)
            {
                var rawObject = Encoding.UTF8.GetString(data);
                _logger.LogWarning(e,
                    "Error during deserializing: {Message}. Raw message: [{RawObject}]",
                    e.Message,
                    rawObject);
                throw;
            }
        }
    }
}