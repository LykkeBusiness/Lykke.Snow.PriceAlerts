using System;
using System.Linq;
using Autofac;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Configuration.BoundedContext;
using Lykke.Cqrs.Configuration.Routing;
using Lykke.Cqrs.Middleware.Logging;
using Lykke.Messaging.Serialization;
using Lykke.Snow.Common.Correlation.Cqrs;
using Lykke.Snow.Common.Startup;
using Lykke.Snow.Cqrs;
using Lykke.Snow.PriceAlerts.Contract.Models.Events;
using Lykke.Snow.PriceAlerts.Projections;
using Lykke.Snow.PriceAlerts.Settings;
using MarginTrading.AssetService.Contracts.Products;
using MarginTrading.Backend.Contracts.TradingSchedule;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Lykke.Snow.PriceAlerts.Modules
{
    public class CqrsModule : Module
    {
        private const string DefaultRoute = "self";
        private const string DefaultPipeline = "commands";
        private const string DefaultEventPipeline = "events";
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly long _defaultRetryDelayMs;
        private readonly CqrsSettings _settings;

        public CqrsModule(CqrsSettings settings)
        {
            _settings = settings;
            _contextNames = settings.ContextNames;
            _defaultRetryDelayMs = (long) _settings.RetryDelay.TotalMilliseconds;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>()
                .SingleInstance();

            builder.RegisterAssemblyTypes(GetType().Assembly)
                .Where(t => new[] {"Saga", "CommandsHandler", "Projection"}
                    .Any(ending => t.Name.EndsWith(ending)))
                .AsSelf();

            builder.Register(CreateEngine)
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx)
        {
            var rabbitMqConventionEndpointResolver = new RabbitMqConventionEndpointResolver(
                "RabbitMq",
                SerializationFormat.MessagePack,
                environment: _settings.EnvironmentName);

            var rabbitMqSettings = new ConnectionFactory
            {
                Uri = new Uri(_settings.ConnectionString, UriKind.Absolute)
            };

            var log = new LykkeLoggerAdapter<CqrsModule>(ctx.Resolve<ILogger<CqrsModule>>());
            var engine = new RabbitMqCqrsEngine(log,
                ctx.Resolve<IDependencyResolver>(),
                new DefaultEndpointProvider(),
                rabbitMqSettings.Endpoint.ToString(),
                rabbitMqSettings.UserName,
                rabbitMqSettings.Password,
                true,
                Register.DefaultEndpointResolver(rabbitMqConventionEndpointResolver),
                RegisterDefaultRouting(),
                RegisterContext(),
                Register.CommandInterceptors(new DefaultCommandLoggingInterceptor(log)),
                Register.EventInterceptors(new DefaultEventLoggingInterceptor(log)));

            var correlationManager = ctx.Resolve<CqrsCorrelationManager>();
            engine.SetWriteHeadersFunc(correlationManager.BuildCorrelationHeadersIfExists);
            engine.SetReadHeadersAction(correlationManager.FetchCorrelationIfExists);
            engine.StartPublishers();

            return engine;
        }

        private PublishingCommandsDescriptor<IDefaultRoutingRegistration> RegisterDefaultRouting()
        {
            return Register.DefaultRouting
                .PublishingCommands(
                )
                .To(_contextNames.PriceAlertsService)
                .With(DefaultPipeline);
        }

        private IRegistration RegisterContext()
        {
            var contextRegistration = Register.BoundedContext(_contextNames.PriceAlertsService)
                .FailedCommandRetryDelay(_defaultRetryDelayMs)
                .ProcessingOptions(DefaultRoute).MultiThreaded(8).QueueCapacity(1024);

            RegisterEventPublishing(contextRegistration);

            RegisterProductProjection(contextRegistration);
            RegisterExpirationProcessProjection(contextRegistration);

            return contextRegistration;
        }

        private static void RegisterEventPublishing(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration.PublishingEvents(typeof(PriceAlertTriggeredEvent)
                )
                .With(DefaultEventPipeline);
        }

        private void RegisterProductProjection(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration.ListeningEvents(
                    typeof(ProductChangedEvent))
                .From(_settings.ContextNames.SettingsService)
                .On(nameof(ProductChangedEvent))
                .WithProjection(
                    typeof(ProductProjection), _settings.ContextNames.SettingsService);
        }
        
        private void RegisterExpirationProcessProjection(
            ProcessingOptionsDescriptor<IBoundedContextRegistration> contextRegistration)
        {
            contextRegistration.ListeningEvents(
                    typeof(ExpirationProcessStartedEvent))
                .From(_settings.ContextNames.TradingEngine)
                .On(nameof(ExpirationProcessStartedEvent))
                .WithProjection(
                    typeof(ExpirationProcessProjection), _settings.ContextNames.TradingEngine);
        }
    }
}