using System;
using Lykke.Snow.Common.Extensions;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lykke.Snow.PriceAlerts.SqlRepositories.EntityConfigurations
{
    public class PriceAlertEntityConfiguration : IEntityTypeConfiguration<PriceAlertEntity>
    {
        private const string DbDecimal = "decimal(18,8)";

        public void Configure(EntityTypeBuilder<PriceAlertEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Price).IsRequired().HasColumnType(DbDecimal);
            builder.Property(x => x.Direction).IsRequired();
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.AccountId).IsRequired();

            builder.Property(x => x.CreatedOn)
                .IsRequired()
                .HasConversion((x) => x.AssumeUtcIfUnspecified(),
                    (x) => x.AssumeUtcIfUnspecified());
            builder.Property(x => x.ModifiedOn)
                .IsRequired()
                .HasConversion((x) => x.AssumeUtcIfUnspecified(),
                    (x) => x.AssumeUtcIfUnspecified());

            builder.Property(x => x.Validity)
                .HasConversion(x => x.HasValue ? x.Value.AssumeUtcIfUnspecified() : x,
                    x => x.HasValue ? x.Value.AssumeUtcIfUnspecified() : x);

            builder.Property(x => x.PriceType).IsRequired();
            builder.Property(x => x.ProductId).IsRequired();

            builder.Property(x => x.Comment).HasMaxLength(PriceAlertsConstants.MaxCommentLength);

            builder
                .Property(e => e.PriceType)
                .HasConversion(
                    v => v.ToString(),
                    v => (PriceType) Enum.Parse(typeof(PriceType), v));

            builder
                .Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (AlertStatus) Enum.Parse(typeof(AlertStatus), v));

            builder
                .Property(e => e.Direction)
                .HasConversion(
                    v => v.ToString(),
                    v => (CrossingDirection) Enum.Parse(typeof(CrossingDirection), v));
        }
    }
}