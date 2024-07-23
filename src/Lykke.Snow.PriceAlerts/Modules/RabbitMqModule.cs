using Autofac;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Snow.Common.Correlation.RabbitMq;
using Lykke.Snow.PriceAlerts.Extensions;
using Lykke.Snow.PriceAlerts.ExternalContracts;
using Lykke.Snow.PriceAlerts.MessageHandlers;
using Lykke.Snow.PriceAlerts.Settings;

namespace Lykke.Snow.PriceAlerts.Modules
{
    public class RabbitMqModule : Module
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMqModule(RabbitMqSettings settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.AddRabbitMqConnectionProvider();

            builder.AddRabbitMqListener<BidAskPairRabbitMqContract, QuotesHandler>(
                    _settings.Consumers.QuotesRabbitMqSettings.ToInstanceSubscriptionSettings(null, false), 
                    (s, p) =>
                    {
                        var correlationManager = p.Resolve<RabbitMqCorrelationManager>();
                        s.SetReadHeadersAction(correlationManager.FetchCorrelationIfExists);
                    })
                .AddOptions(opt =>
                {
                    opt.SerializationFormat = SerializationFormat.Json;
                    opt.ShareConnection = false;
                    opt.SubscriptionTemplate = SubscriptionTemplate.LossAcceptable;
                    opt.ConsumerCount = (byte)_settings.Consumers.QuotesRabbitMqSettings.ConsumerCount;
                });
        }
    }
}