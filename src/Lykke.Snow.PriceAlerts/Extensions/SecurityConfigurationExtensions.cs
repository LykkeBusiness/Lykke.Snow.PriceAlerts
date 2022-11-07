using System.Net.Http;
using IdentityModel.Client;
using Lykke.Snow.Common.Startup;
using Lykke.Snow.Common.Startup.Authorization;
using Lykke.Snow.PriceAlerts.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.Extensions
{
    public static class SecurityConfigurationExtensions
    {
        /// <summary>
        /// Adds identity model TokenClient and <see cref="AccessTokenDelegatingHandler"/> into DI container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="authority"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="clientScope"></param>
        /// <param name="renewTokenTimeoutSec"></param>
        /// <param name="validateIssuerName"></param>
        /// <param name="requireHttps"></param>
        public static void AddDelegatingHandler(
            this IServiceCollection services,
            string authority,
            string clientId,
            string clientSecret,
            string clientScope,
            int? renewTokenTimeoutSec,
            bool validateIssuerName,
            bool requireHttps)
        {
            services.AddTokenClient(authority, clientId, clientSecret, validateIssuerName, requireHttps);

            services.AddSingleton(provider => new AccessTokenDelegatingHandler(
                provider.GetService<TokenClient>(),
                clientScope,
                new HttpClientHandler(),
                provider.GetService<ILogger<AccessTokenDelegatingHandler>>(),
                renewTokenTimeoutSec));

            services.AddSingleton<HttpMessageHandler>(provider => provider.GetService<AccessTokenDelegatingHandler>());
        }

        public static void AddDelegatingHandler(this IServiceCollection services, IConfiguration configuration)
        {
            var authority = configuration.GetValue<string>("Api-Authority");
            var clientId = configuration.GetValue<string>("Client-Id");
            var clientSecret = configuration.GetValue<string>("Client-Secret");
            var clientScope = configuration.GetValue<string>("Client-Scope");
            var validateIssuerName = configuration.GetValue<bool>("Validate-Issuer-Name");
            var requireHttps = configuration.GetValue<bool>("Require-Https");
            var renewTokenTimeoutSec = configuration.GetValue<int>("Renew-Token-Timeout-Sec");
            
            services.AddDelegatingHandler(authority,
                clientId,
                clientSecret,
                clientScope,
                renewTokenTimeoutSec,
                validateIssuerName,
                requireHttps);
        }
    }
}