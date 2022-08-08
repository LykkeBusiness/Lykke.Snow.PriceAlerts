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
        private readonly AppSettings _appSettings;

        public ClientsModule(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterClientWithName<IProductsApi>(builder, "Asset", _appSettings.PriceAlerts.AssetService);
            RegisterClientWithName<IPricesApi>(builder, "MT Core", _appSettings.PriceAlerts.TradingCore);
            
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