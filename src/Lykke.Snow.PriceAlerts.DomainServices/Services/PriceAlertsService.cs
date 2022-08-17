using System;
using System.Collections.Generic;
using System.Linq;
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

            if (priceAlert.Price <= 0)
                return new Result<PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidPrice);

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
            
            priceAlert.Id = cachedAlert.Id;
            priceAlert.CreatedOn = cachedAlert.CreatedOn;
            priceAlert.Direction = cachedAlert.Direction;
            priceAlert.PriceType = cachedAlert.PriceType;
            priceAlert.ProductId = cachedAlert.ProductId;
            priceAlert.CorrelationId = cachedAlert.CorrelationId;
            
            if (priceAlert.Price <= 0)
                return new Result<PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidPrice);

            var isUnique = _priceAlertsCache.IsUnique(priceAlert);
            if (!isUnique) return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.Duplicate);

            if (!string.IsNullOrEmpty(priceAlert.Comment) && priceAlert.Comment.Length > 70)
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.CommentTooLong);

            if (priceAlert.Validity.HasValue && priceAlert.Validity.Value <= _systemClock.Now())
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidValidity);

            if (string.IsNullOrEmpty(priceAlert.ProductId) || !_productsCache.Contains(priceAlert.ProductId))
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidProduct);

            
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

        public async Task<Result<PriceAlertErrorCodes>> TriggerAsync(string id)
        {
            var isActive = _priceAlertsCache.IsActive(id, out var cachedAlert);
            if (!isActive) return new Result<PriceAlertErrorCodes>(PriceAlertErrorCodes.DoesNotExist);

            cachedAlert.ModifiedOn = _systemClock.Now();
            cachedAlert.Status = AlertStatus.Triggered;
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

        public ValueTask<IEnumerable<PriceAlert>> GetActiveByProductId(string productId)
        {
            return _priceAlertsCache.GetActiveByProductId(productId);
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

        public async Task ExpireAllAsync(DateTime expirationDate)
        {
            _logger.LogInformation("Starting to expire price alerts older than {ExpirationDate}", expirationDate);
            var activePriceAlerts = await _priceAlertsCache.GetAllActiveAlerts();
            var expiredPriceAlerts = activePriceAlerts.Where(x => x.Validity.HasValue &&
                                                                  x.Validity < expirationDate);

            foreach (var priceAlert in expiredPriceAlerts)
            {
                var result = await ExpireAsync(priceAlert.Id);
                if (result.IsFailed)
                {
                    _logger.LogWarning("Could not expire price alert {Id}, reason {Reason}",
                        priceAlert.Id,
                        result.Error);
                }
            }
        }
    }
}