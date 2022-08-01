using System.Threading.Tasks;
using Lykke.Snow.Common.Model;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.DomainServices.Services
{
    public class PriceAlertsService : IPriceAlertsService
    {
        private readonly ILogger<PriceAlertsService> _logger;
        private readonly IPriceAlertsCache _priceAlertsCache;
        private readonly IProductsCache _productsCache;
        private readonly ISystemClock _systemClock;

        public PriceAlertsService(IPriceAlertsCache priceAlertsCache,
            IProductsCache productsCache,
            ISystemClock systemClock,
            ILogger<PriceAlertsService> logger)
        {
            _priceAlertsCache = priceAlertsCache;
            _productsCache = productsCache;
            _systemClock = systemClock;
            _logger = logger;
        }

        public async Task<Result<PriceAlert, PriceAlertErrorCodes>> GetByIdAsync(string id)
        {
            var isSuccess = PriceAlertId.TryParse(id, out var result);
            if (!isSuccess) return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidId);

            return await _priceAlertsCache.GetByIdAsync(result);
        }

        public async Task<Result<PriceAlertErrorCodes>> InsertAsync(PriceAlert priceAlert)
        {
            var isUnique = _priceAlertsCache.IsUnique(priceAlert);
            if (!isUnique) return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.Duplicate);

            if (!string.IsNullOrEmpty(priceAlert.Comment) && priceAlert.Comment.Length > 70)
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.CommentTooLong);

            if (priceAlert.Validity.HasValue && priceAlert.Validity.Value <= _systemClock.Now())
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidValidity);

            if (string.IsNullOrEmpty(priceAlert.ProductId) || !_productsCache.Contains(priceAlert.ProductId))
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidProduct);

            priceAlert.Id = new PriceAlertId();
            priceAlert.CreatedOn = _systemClock.Now();
            priceAlert.ModifiedOn = _systemClock.Now();
            var result = await _priceAlertsCache.InsertAsync(priceAlert);

            // TODO: cqrs

            return result;
        }

        public async Task<Result<PriceAlertErrorCodes>> UpdateAsync(PriceAlert priceAlert)
        {
            var isActive = _priceAlertsCache.IsActive(priceAlert.Id, out var cachedAlert);
            if (!isActive) return new Result<PriceAlertErrorCodes>(PriceAlertErrorCodes.DoesNotExist);

            var isUnique = _priceAlertsCache.IsUnique(priceAlert);
            if (!isUnique) return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.Duplicate);

            if (!string.IsNullOrEmpty(priceAlert.Comment) && priceAlert.Comment.Length > 70)
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.CommentTooLong);

            if (priceAlert.Validity.HasValue && priceAlert.Validity.Value <= _systemClock.Now())
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidValidity);

            if (string.IsNullOrEmpty(priceAlert.ProductId) || !_productsCache.Contains(priceAlert.ProductId))
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidProduct);

            priceAlert.Id = new PriceAlertId();
            priceAlert.CreatedOn = cachedAlert.CreatedOn;
            priceAlert.ModifiedOn = _systemClock.Now();
            var result = await _priceAlertsCache.UpdateAsync(priceAlert);

            // TODO: cqrs

            return result;
        }

        public async Task<Result<PriceAlertErrorCodes>> CancelAsync(string id)
        {
            var isActive = _priceAlertsCache.IsActive(id, out var cachedAlert);
            if (!isActive) return new Result<PriceAlertErrorCodes>(PriceAlertErrorCodes.DoesNotExist);

            cachedAlert.ModifiedOn = _systemClock.Now();
            cachedAlert.Status = AlertStatus.Cancelled;
            var result = await _priceAlertsCache.UpdateAsync(cachedAlert);

            // TODO: cqrs

            return result;
        }

        public Task<PaginatedResponse<PriceAlert>> GetByPageAsync(string accountId, string productId,
            AlertStatus? status, int skip, int take)
        {
            return _priceAlertsCache.GetByPageAsync(accountId, productId, status, skip, take);
        }

        public async Task CancelByProductIdAsync(string productId)
        {
            var alerts = await _priceAlertsCache.GetActiveByProductId(productId);
            foreach (var alert in alerts)
            {
                alert.ModifiedOn = _systemClock.Now();
                alert.Status = AlertStatus.Cancelled;
                await _priceAlertsCache.UpdateAsync(alert);
            }
        }

        public async Task<Result<PriceAlertErrorCodes>> ExpireAsync(string id)
        {
            var isActive = _priceAlertsCache.IsActive(id, out var cachedAlert);
            if (!isActive) return new Result<PriceAlertErrorCodes>(PriceAlertErrorCodes.DoesNotExist);

            cachedAlert.ModifiedOn = _systemClock.Now();
            cachedAlert.Status = AlertStatus.Expired;
            var result = await _priceAlertsCache.UpdateAsync(cachedAlert);

            // TODO: cqrs

            return result;
        }
    }
}