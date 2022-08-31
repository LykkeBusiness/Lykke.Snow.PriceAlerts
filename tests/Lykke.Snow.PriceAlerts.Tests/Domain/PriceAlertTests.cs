using Lykke.Snow.PriceAlerts.Domain.Models;
using Xunit;

namespace Lykke.Snow.PriceAlerts.Tests.Domain
{
    public class PriceAlertTests
    {
        [Fact]
        public void TwoAlertsWithDifferentId_AreDuplicated()
        {
            var alert1 = new PriceAlert()
            {
                Id = new PriceAlertId(),
                ProductId = "product",
                AccountId = "AA0010",
                Price = 0.99m,
                PriceType = PriceType.Mid,
                Direction = CrossingDirection.Up,
                Status = AlertStatus.Active
            };

            var alert2 = alert1.ShallowCopy();
            alert2.Id = new PriceAlertId();

            var isDuplicate = alert1.IsDuplicateOf(alert2);

            Assert.True(isDuplicate);
        }
        
        /// <summary>
        /// If ids are same, we must skip duplication validation
        /// (because it is an update operation)
        /// </summary>
        [Fact]
        public void TwoAlertsWithSameId_AreNotDuplicated()
        {
            var alert1 = new PriceAlert()
            {
                Id = new PriceAlertId(),
                ProductId = "product",
                AccountId = "AA0010",
                Price = 0.99m,
                PriceType = PriceType.Mid,
                Direction = CrossingDirection.Up,
                Status = AlertStatus.Active
            };

            var alert2 = alert1.ShallowCopy();

            var isDuplicate = alert1.IsDuplicateOf(alert2);

            Assert.False(isDuplicate);
        }
    }
}