using System.Data.Common;
using JetBrains.Annotations;
using Lykke.Common.MsSql;
using Lykke.Snow.PriceAlerts.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Snow.PriceAlerts.SqlRepositories
{
    public class PriceAlertsDbContext : MsSqlContext
    {
        private const string Schema = "priceAlerts";

        // Used for EF migrations
        [UsedImplicitly]
        public PriceAlertsDbContext() : base(Schema)
        {
        }

        public PriceAlertsDbContext(string connectionString, bool isTraceEnabled)
            : base(Schema, connectionString, isTraceEnabled)
        {
        }

        public PriceAlertsDbContext(DbContextOptions contextOptions) : base(Schema, contextOptions)
        {
        }

        public PriceAlertsDbContext(DbConnection dbConnection) : base(Schema, dbConnection)
        {
        }

        internal DbSet<PriceAlertEntity> PriceAlerts { get; set; }

        protected override void OnLykkeModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PriceAlertsDbContext).Assembly);
        }
    }
}