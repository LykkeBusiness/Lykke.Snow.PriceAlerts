using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Services;
using MarginTrading.AssetService.Contracts;
using MarginTrading.AssetService.Contracts.Products;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.DomainServices.Caches
{
    public class ProductsCache : IProductsCache
    {
        private readonly ILogger<ProductsCache> _logger;
        private readonly IMapper _mapper;
        private readonly IProductsApi _productsApi;

        private readonly ConcurrentDictionary<string, ProductCacheModel> _cache =
            new ConcurrentDictionary<string, ProductCacheModel>();

        public ProductsCache(IProductsApi productsApi,
            IMapper mapper,
            ILogger<ProductsCache> logger)
        {
            _productsApi = productsApi;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Init()
        {
            var response = await _productsApi.GetAllAsync(new GetProductsRequest
            {
                Skip = 0,
                Take = 0,
                IsDiscontinued = false,
                IsStarted = true
            });

            var products = response.Products
                .Select(x => _mapper.Map<ProductContract, ProductCacheModel>(x))
                .ToList();

            foreach (var product in products)
                _cache.AddOrUpdate(product.ProductId, product, (key, oldValue) => product);
        }

        public bool Contains(string productId)
        {
            return _cache.ContainsKey(productId);
        }
        
        public ProductCacheModel Get(string productId)
        {
            var isSuccess = _cache.TryGetValue(productId, out var result);
            return isSuccess ? result : null;
        }

        public void Remove(string productId)
        {
            _cache.TryRemove(productId, out _);
        }

        public void AddOrUpdate(ProductCacheModel product)
        {
            _logger.LogInformation($"Product cache update: {product.ProductId}");
            _cache.AddOrUpdate(product.ProductId, product, (key, oldValue) => product);
        }
    }
}