using Autofac;
using Lykke.Common.MsSql;
using Lykke.Snow.PriceAlerts.Domain.Repositories;
using Lykke.Snow.PriceAlerts.SqlRepositories;
using Lykke.Snow.PriceAlerts.SqlRepositories.Repositories;

namespace Lykke.Snow.PriceAlerts.Modules
{
    public class DataModule : Module
    {
        private readonly string _connectionString;

        public DataModule(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMsSql(_connectionString,
                connString => new PriceAlertsDbContext(connString, false),
                dbConn => new PriceAlertsDbContext(dbConn));

            builder.RegisterType<PriceAlertsRepository>()
                .As<IPriceAlertsRepository>()
                .SingleInstance();
        }
    }
}