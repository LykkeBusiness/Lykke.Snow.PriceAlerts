using System;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Services;
using MarginTrading.AssetService.Contracts.Enums;
using MarginTrading.AssetService.Contracts.Products;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.Projections
{
    public class ProductProjection
    {
        private readonly IProductsCache _cache;
        private readonly ILogger<ProductProjection> _logger;
        private readonly IMapper _mapper;
        private readonly IPriceAlertsService _priceAlertsService;

        public ProductProjection(IProductsCache cache,
            IPriceAlertsService priceAlertsService,
            IMapper mapper,
            ILogger<ProductProjection> logger)
        {
            _cache = cache;
            _priceAlertsService = priceAlertsService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Handle(ProductChangedEvent @event)
        {
            switch (@event.ChangeType)
            {
                case ChangeType.Creation:
                case ChangeType.Edition:
                    await HandleEdition(@event);
                    break;
                case ChangeType.Deletion:
                    await HandleDeletion(@event);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task HandleDeletion(ProductChangedEvent @event)
        {
            var productId = @event.OldValue.ProductId;
            _logger.LogWarning("Product {Id} is deleted. Cancelling all alerts", productId);
            await Remove(productId);
        }

        private async Task HandleEdition(ProductChangedEvent @event)
        {
            var newProduct = @event.NewValue;
            if (newProduct.IsDiscontinued || !newProduct.IsStarted)
            {
                var productId = newProduct.ProductId;
                _logger.LogWarning("Product {Id} is discontinued or stopped. Cancelling all alerts", productId);
                await Remove(newProduct.ProductId);
            }
            else
            {
                var cacheModel = _mapper.Map<ProductContract, ProductCacheModel>(newProduct);
                _cache.AddOrUpdate(cacheModel);
            }
        }

        private async Task Remove(string productId)
        {
            _cache.Remove(productId);
            await _priceAlertsService.CancelByProductIdAsync(productId);
        }
    }
}