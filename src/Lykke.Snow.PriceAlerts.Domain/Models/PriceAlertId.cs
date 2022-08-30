using System;

namespace Lykke.Snow.PriceAlerts.Domain.Models
{
    public class PriceAlertId
    {
        public PriceAlertId() : this(Guid.NewGuid())
        {
        }

        public PriceAlertId(Guid id)
        {
            Id = Format(id);
        }

        public string Id { get; }

        public static bool TryParse(string value, out PriceAlertId id)
        {
            var isSuccess = Guid.TryParse(value, out var guid);

            id = isSuccess ? new PriceAlertId(guid) : null;

            return isSuccess;
        }

        private string Format(Guid id)
        {
            return id.ToString("N");
        }

        public override string ToString()
        {
            return Id;
        }

        public static implicit operator string(PriceAlertId priceAlertId)
        {
            return priceAlertId.Id;
        }
    }
}