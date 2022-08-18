using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Snow.Common.Model;
using Lykke.Snow.PriceAlerts.Domain.Extensions;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Repositories;
using Lykke.Snow.PriceAlerts.Domain.Services;

namespace Lykke.Snow.PriceAlerts.DomainServices.Caches
{
    public class PriceAlertsCache : IPriceAlertsCache
    {
        private readonly IPriceAlertsRepository _repository;

        private ConcurrentDictionary<string, PriceAlert> _cache =
            new ConcurrentDictionary<string, PriceAlert>();

        public PriceAlertsCache(IPriceAlertsRepository repository)
        {
            _repository = repository;
        }

        public async Task Init()
        {
            var values = await _repository.GetAllActiveAlerts();
            _cache = new ConcurrentDictionary<string, PriceAlert>(values.Select(x =>
                new KeyValuePair<string, PriceAlert>(x.Id, x)));
        }

        public Task<Result<PriceAlert, PriceAlertErrorCodes>> GetByIdAsync(string id)
        {
            return _repository.GetByIdAsync(id);
        }

        public async Task<Result<PriceAlertErrorCodes>> InsertAsync(PriceAlert priceAlert)
        {
            var result = await _repository.InsertAsync(priceAlert);
            if (result.IsSuccess) _cache.AddOrUpdate(priceAlert.Id, priceAlert, (key, oldValue) => priceAlert);

            return result;
        }

        public async Task<Result<PriceAlertErrorCodes>> UpdateAsync(PriceAlert priceAlert)
        {
            var isActive = priceAlert.IsActive();
            var result = await _repository.UpdateAsync(priceAlert);
            if (result.IsSuccess)
            {
                if (isActive)
                    _cache.AddOrUpdate(priceAlert.Id, priceAlert, (key, oldValue) => priceAlert);
                else
                    _cache.TryRemove(priceAlert.Id, out _);
            }

            return result;
        }

        public Task<PaginatedResponse<PriceAlert>> GetByPageAsync(string accountId, string productId,
            AlertStatus? status, int skip, int take)
        {
            return _repository.GetByPageAsync(accountId, productId, status, skip, take);
        }

        public Task<List<PriceAlert>> GetAllActiveAlerts()
        {
            return Task.FromResult(_cache.Values.ToList());
        }

        public ValueTask<IEnumerable<PriceAlert>> GetActiveByProductId(string productId)
        {
            return new ValueTask<IEnumerable<PriceAlert>>(_cache.Values.Where(x => x.ProductId == productId));
        }

        public bool IsUnique(PriceAlert priceAlert)
        {
            var notUnique = _cache.Values.Any(x => x.SameAs(priceAlert));
            return !notUnique;
        }

        public bool IsActive(string id, out PriceAlert cachedAlert)
        {
            var isSuccess = _cache.TryGetValue(id, out cachedAlert);
            return isSuccess;
        }
    }
}