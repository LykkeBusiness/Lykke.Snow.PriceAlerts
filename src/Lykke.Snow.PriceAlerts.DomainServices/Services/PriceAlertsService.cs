using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Snow.Common.Extensions;
using Lykke.Snow.Common.Model;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;
using Lykke.Snow.PriceAlerts.Contract.Models.Events;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Services;
using Meteor.Client;
using Meteor.Client.Models;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.DomainServices.Services
{
    public class PriceAlertsService : IPriceAlertsService
    {
        private readonly ILogger<PriceAlertsService> _logger;
        private readonly IPriceAlertsCache _priceAlertsCache;
        private readonly IProductsCache _productsCache;
        private readonly ISystemClock _systemClock;
        private readonly ICqrsEntityChangedSender _entityChangedSender;
        private readonly IMeteorService _meteorService;

        public PriceAlertsService(IPriceAlertsCache priceAlertsCache,
            IProductsCache productsCache,
            ISystemClock systemClock,
            ICqrsEntityChangedSender entityChangedSender,
            IMeteorService meteorService,
            ILogger<PriceAlertsService> logger) 

        {
            _priceAlertsCache = priceAlertsCache;
            _productsCache = productsCache;
            _systemClock = systemClock;
            _entityChangedSender = entityChangedSender;
            _meteorService = meteorService;
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

            if (!string.IsNullOrEmpty(priceAlert.Comment) &&
                priceAlert.Comment.Length > PriceAlertsConstants.MaxCommentLength)
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.CommentTooLong);

            if (priceAlert.Validity.HasValue &&
                priceAlert.Validity.Value.AssumeUtcIfUnspecified().Date < _systemClock.UtcNow().Date)
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidValidity);

            if (string.IsNullOrEmpty(priceAlert.ProductId) || !_productsCache.Contains(priceAlert.ProductId))
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidProduct);

            priceAlert.Id = new PriceAlertId();
            priceAlert.CreatedOn = _systemClock.UtcNow();
            priceAlert.ModifiedOn = _systemClock.UtcNow();
            var result = await _priceAlertsCache.InsertAsync(priceAlert);

            if (result.IsSuccess)
            {
                var context = await GetContext(priceAlert);
                await _entityChangedSender
                    .SendEntityCreatedEvent<PriceAlert, PriceAlertContract, PriceAlertContext, PriceAlertContextContract
                        , PriceAlertChangedEvent>(priceAlert, context);
            }

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

            if (!string.IsNullOrEmpty(priceAlert.Comment) &&
                priceAlert.Comment.Length > PriceAlertsConstants.MaxCommentLength)
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.CommentTooLong);

            if (priceAlert.Validity.HasValue &&
                priceAlert.Validity.Value.AssumeUtcIfUnspecified().Date < _systemClock.UtcNow().Date)
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidValidity);

            if (string.IsNullOrEmpty(priceAlert.ProductId) || !_productsCache.Contains(priceAlert.ProductId))
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.InvalidProduct);


            priceAlert.ModifiedOn = _systemClock.UtcNow();
            var result = await _priceAlertsCache.UpdateAsync(priceAlert);

            if (result.IsSuccess)
            {
                var context = await GetContext(priceAlert);
                await _entityChangedSender
                    .SendEntityEditedEvent<PriceAlert, PriceAlertContract, PriceAlertContext, PriceAlertContextContract,
                        PriceAlertChangedEvent>(cachedAlert,
                        priceAlert, context);
            }

            return result;
        }

        public async Task<Result<PriceAlertErrorCodes>> CancelAsync(string id)
        {
            var isActive = _priceAlertsCache.IsActive(id, out var cachedAlert);
            if (!isActive) return new Result<PriceAlertErrorCodes>(PriceAlertErrorCodes.DoesNotExist);

            var priceAlert = cachedAlert.ShallowCopy();
            priceAlert.ModifiedOn = _systemClock.UtcNow();
            priceAlert.Status = AlertStatus.Cancelled;
            var result = await _priceAlertsCache.UpdateAsync(priceAlert);

            if (result.IsSuccess)
            {
                var context = await GetContext(priceAlert);
                await _entityChangedSender
                    .SendEntityEditedEvent<PriceAlert, PriceAlertContract, PriceAlertContext, PriceAlertContextContract,
                        PriceAlertChangedEvent>(cachedAlert,
                        priceAlert,
                        context);
            }

            return result;
        }

        public async Task<Result<PriceAlertErrorCodes>> TriggerAsync(string id)
        {
            var isActive = _priceAlertsCache.IsActive(id, out var cachedAlert);
            if (!isActive) return new Result<PriceAlertErrorCodes>(PriceAlertErrorCodes.DoesNotExist);

            var priceAlert = cachedAlert.ShallowCopy();
            priceAlert.ModifiedOn = _systemClock.UtcNow();
            priceAlert.Status = AlertStatus.Triggered;
            var result = await _priceAlertsCache.UpdateAsync(priceAlert);

            if (result.IsSuccess)
            {
                var context = await GetContext(priceAlert);
                await _entityChangedSender
                    .SendEntityEditedEvent<PriceAlert, PriceAlertContract, PriceAlertContext, PriceAlertContextContract,
                        PriceAlertChangedEvent>(cachedAlert,
                        priceAlert, context);

                await _meteorService.SendPriceAlertTriggered(priceAlert);
            }

            return result;
        }

        public Task<PaginatedResponse<PriceAlert>> GetByPageAsync(string accountId, string productId,
            List<AlertStatus> statuses, int skip, int take)
        {
            return _priceAlertsCache.GetByPageAsync(accountId, productId, statuses, skip, take);
        }

        public async Task<int> CancelByProductAndAccountAsync(string productId, string accountId)
        {
            var alerts = await _priceAlertsCache.GetAllActiveAlerts();
            if (!string.IsNullOrEmpty(productId))
            {
                alerts = alerts.Where(x => x.ProductId == productId);
            }

            if (!string.IsNullOrEmpty(accountId))
            {
                alerts = alerts.Where(x => x.AccountId == accountId);
            }

            var cancelled = 0;

            foreach (var cachedAlert in alerts)
            {
                var priceAlert = cachedAlert.ShallowCopy();
                priceAlert.ModifiedOn = _systemClock.UtcNow();
                priceAlert.Status = AlertStatus.Cancelled;
                var result = await _priceAlertsCache.UpdateAsync(priceAlert);

                if (result.IsSuccess)
                {
                    cancelled++;

                    var context = await GetContext(priceAlert);
                    await _entityChangedSender
                        .SendEntityEditedEvent<PriceAlert, PriceAlertContract, PriceAlertContext,
                            PriceAlertContextContract, PriceAlertChangedEvent>(cachedAlert,
                            priceAlert, context);
                }
            }

            return cancelled;
        }

        public async Task<Result<PriceAlertErrorCodes>> ExpireAsync(string id)
        {
            var isActive = _priceAlertsCache.IsActive(id, out var cachedAlert);
            if (!isActive) return new Result<PriceAlertErrorCodes>(PriceAlertErrorCodes.DoesNotExist);

            var priceAlert = cachedAlert.ShallowCopy();
            priceAlert.ModifiedOn = _systemClock.UtcNow();
            priceAlert.Status = AlertStatus.Expired;
            var result = await _priceAlertsCache.UpdateAsync(priceAlert);

            if (result.IsSuccess)
            {
                var context = await GetContext(priceAlert);
                await _entityChangedSender
                    .SendEntityEditedEvent<PriceAlert, PriceAlertContract, PriceAlertContext, PriceAlertContextContract,
                        PriceAlertChangedEvent>(cachedAlert,
                        priceAlert, context);
            }

            return result;
        }

        public async Task ExpireAllAsync(DateTime expirationDate)
        {
            _logger.LogInformation("Starting to expire price alerts older than {ExpirationDate}", expirationDate);
            var activePriceAlerts = await _priceAlertsCache.GetAllActiveAlerts();
            var expiredPriceAlerts = activePriceAlerts.Where(x => x.Validity.HasValue &&
                                                                  x.Validity.Value.AssumeUtcIfUnspecified().Date <
                                                                  expirationDate.Date);

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

        public async Task<List<string>> GetProductsWithActiveAlertsAsync(string accountId)
        {
            var all = await _priceAlertsCache.GetAllActiveAlerts();

            var products = all
                .Where(x => x.AccountId == accountId)
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            return products;
        }

        private async Task<PriceAlertContext> GetContext(PriceAlert priceAlert)
        {
            var accountId = priceAlert.AccountId;
            var productId = priceAlert.ProductId;
            var all = await _priceAlertsCache.GetAllActiveAlerts();
            var hasActiveAlerts = all.Any(x => x.AccountId == accountId && x.ProductId == productId);

            return new PriceAlertContext()
            {
                HasActiveAlerts = hasActiveAlerts,
            };
        }
    }
}