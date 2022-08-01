// Copyright (c) 2020 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Autofac;
using Lykke.Middlewares.Mappers;
using Lykke.Snow.PriceAlerts.Domain.Services;
using Lykke.Snow.PriceAlerts.DomainServices.Caches;
using Lykke.Snow.PriceAlerts.DomainServices.Services;
using Lykke.Snow.PriceAlerts.Services;

namespace Lykke.Snow.PriceAlerts.Modules
{
    internal class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultHttpStatusCodeMapper>()
                .As<IHttpStatusCodeMapper>()
                .SingleInstance();

            builder.RegisterType<DefaultLogLevelMapper>()
                .As<ILogLevelMapper>()
                .SingleInstance();

            builder.RegisterType<PriceAlertsService>()
                .As<IPriceAlertsService>()
                .SingleInstance();

            builder.RegisterType<SystemClock>()
                .As<ISystemClock>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ProductsCache>()
                .As<IProductsCache>()
                .SingleInstance();

            builder.RegisterType<PriceAlertsCache>()
                .As<IPriceAlertsCache>()
                .SingleInstance();
        }
    }
}