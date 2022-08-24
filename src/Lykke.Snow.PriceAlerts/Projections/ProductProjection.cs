using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Models.InternalCommands;
using Lykke.Snow.PriceAlerts.Domain.Services;
using MarginTrading.AssetService.Contracts.Enums;
using MarginTrading.AssetService.Contracts.Products;
using Microsoft.Extensions.Logging;

namespace Lykke.Snow.PriceAlerts.Projections
{
    public class ProductProjection
    {
        private readonly IProductsCache _cache;
        private readonly IObserver<CancelPriceAlertsCommand> _observer;
        private readonly ILogger<ProductProjection> _logger;
        private readonly IMapper _mapper;

        public ProductProjection(IProductsCache cache,
            IObserver<CancelPriceAlertsCommand> observer,
            IMapper mapper,
            ILogger<ProductProjection> logger)
        {
            _cache = cache;
            _observer = observer;
            _mapper = mapper;
            _logger = logger;
        }

        [UsedImplicitly]
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
            Remove(productId);
        }

        private async Task HandleEdition(ProductChangedEvent @event)
        {
            var newProduct = @event.NewValue;
            if (newProduct.IsDiscontinued || !newProduct.IsStarted)
            {
                var productId = newProduct.ProductId;
                _logger.LogWarning("Product {Id} is discontinued or stopped. Cancelling all alerts", productId);
                Remove(newProduct.ProductId);
            }
            else
            {
                var cacheModel = _mapper.Map<ProductContract, ProductCacheModel>(newProduct);
                _cache.AddOrUpdate(cacheModel);
            }
        }

        private void Remove(string productId)
        {
            _cache.Remove(productId);
            _observer.OnNext(new CancelPriceAlertsCommand(OriginalEventType.InvalidProduct)
            {
                Products = new List<string>() {productId}
            });
        }
    }
}