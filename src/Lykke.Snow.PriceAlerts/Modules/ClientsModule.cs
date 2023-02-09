using Autofac;
using Lykke.HttpClientGenerator;
using Lykke.Snow.PriceAlerts.Settings;
using MarginTrading.AssetService.Contracts;
using MarginTrading.Backend.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Snow.PriceAlerts.Modules
{
    public class ClientsModule : Module
    {
        private readonly PriceAlertsSettings _settings;

        public ClientsModule(PriceAlertsSettings settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterClientWithName<IProductsApi>(builder, "Asset", _settings.AssetService);
            RegisterClientWithName<ITradingInstrumentsApi>(builder, "Asset", _settings.AssetService);
            RegisterClientWithName<IPricesApi>(builder, "MT Core", _settings.TradingCore);
            
        }

        private static void RegisterClientWithName<TApi>(ContainerBuilder builder, string name,
            ServiceSettings serviceSettings)
            where TApi : class
        {
            builder.RegisterClient<TApi>(serviceSettings.Url,
                config =>
                {
                    var httpClientGeneratorBuilder =
                        config.WithServiceName<ProblemDetails>($"{name} [{serviceSettings.Url}]");

                    if (!string.IsNullOrEmpty(serviceSettings.ApiKey))
                        httpClientGeneratorBuilder = httpClientGeneratorBuilder.WithApiKey(serviceSettings.ApiKey);

                    return httpClientGeneratorBuilder;
                });
        }
    }
}