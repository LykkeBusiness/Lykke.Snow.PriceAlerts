using System;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Xunit;

namespace Lykke.Snow.PriceAlerts.Tests.Domain
{
    public class PriceAlertIdTests
    {
        [Fact]
        public void CorrectId()
        {
            var id = Guid.NewGuid().ToString("N");

            var isSuccess = PriceAlertId.TryParse(id, out var parsedId);

            Assert.True(isSuccess);
            Assert.Equal(id, parsedId);
            Assert.Equal(id, parsedId.ToString());
        }

        [Fact]
        public void IncorrectId()
        {
            var id = "junk";

            var isSuccess = PriceAlertId.TryParse(id, out var parsedId);

            Assert.False(isSuccess);
            Assert.Null(parsedId);
        }
    }
}