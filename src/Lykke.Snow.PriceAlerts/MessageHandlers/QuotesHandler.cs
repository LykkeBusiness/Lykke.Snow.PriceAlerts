using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Services;
using Lykke.Snow.PriceAlerts.ExternalContracts;

namespace Lykke.Snow.PriceAlerts.MessageHandlers
{
    [UsedImplicitly]
    internal sealed class QuotesHandler : IMessageHandler<BidAskPairRabbitMqContract>
    {
        private readonly IQuoteCache _quoteCache;
        private readonly IMapper _mapper;

        public QuotesHandler(IQuoteCache quoteCache, IMapper mapper)
        {
            _quoteCache = quoteCache;
            _mapper = mapper;
        }

        public async Task Handle(BidAskPairRabbitMqContract message)
        {
            var quote = _mapper.Map<BidAskPairRabbitMqContract, QuoteCacheModel>(message);
            await _quoteCache.AddOrUpdate(quote);
        }
    }
}