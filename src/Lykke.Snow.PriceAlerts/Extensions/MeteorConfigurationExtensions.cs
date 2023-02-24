using Lykke.HttpClientGenerator;
using Lykke.Snow.Common.Startup.Authorization;
using Lykke.Snow.Common.Startup.HttpClientGenerator;
using Lykke.Snow.PriceAlerts.Settings;
using Meteor.Client;
using Microsoft.Extensions.DependencyInjection;
using LykkeHttpClientGenerator = Lykke.HttpClientGenerator.HttpClientGenerator;

namespace Lykke.Snow.PriceAlerts.Extensions
{
    public static class MeteorConfigurationExtensions
    {
        public static void AddMeteorClient(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            services.AddSingleton(provider =>
            {
                var settings = serviceProvider.GetService<PriceAlertsSettings>();
                return LykkeHttpClientGenerator
                    .BuildForUrl(settings.MeteorService.Url)
                    .WithServiceName<HttpErrorResponse>("Meteor Service")
                    .WithAdditionalDelegatingHandler(serviceProvider.GetService<AccessTokenDelegatingHandler>())
                    .WithAdditionalDelegatingHandler(
                        serviceProvider.GetService<NotSuccessStatusCodeDelegatingHandler>())
                    .Create()
                    .Generate<IMeteorClient>();
            });
        }
    }
}