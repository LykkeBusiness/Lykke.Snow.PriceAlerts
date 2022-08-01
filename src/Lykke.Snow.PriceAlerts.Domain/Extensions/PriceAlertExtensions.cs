using Lykke.Snow.PriceAlerts.Domain.Models;

namespace Lykke.Snow.PriceAlerts.Domain.Extensions
{
    public static class PriceAlertExtensions
    {
        public static bool IsActive(this PriceAlert alert)
        {
            return alert.Status == AlertStatus.Active;
        }

        public static bool SameAs(this PriceAlert alert, PriceAlert compare)
        {
            return
                alert.ProductId == compare.ProductId &&
                alert.AccountId == compare.AccountId &&
                alert.PriceType == compare.PriceType &&
                alert.Price == compare.Price &&
                alert.Direction == compare.Direction &&
                alert.IsActive() == compare.IsActive();
        }
    }
}