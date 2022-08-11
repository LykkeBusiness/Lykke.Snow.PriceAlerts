using System;
using AutoMapper;
using Lykke.Snow.Common.Converters;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;
using Lykke.Snow.PriceAlerts.Contract.Models.Requests;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.ExternalContracts;
using MarginTrading.AssetService.Contracts.Products;
using MarginTrading.Backend.Contracts.Snow.Prices;

namespace Lykke.Snow.PriceAlerts.MappingProfiles
{
    public class PriceAlertsProfile : Profile
    {
        public PriceAlertsProfile()
        {
            CreateMap<PriceAlert, PriceAlertContract>();
            CreateMap<PriceAlertContract, PriceAlert>()
                .ForMember(x => x.CorrelationId,
                    opt => opt.Ignore());

            CreateMap<AddPriceAlertRequest, PriceAlert>();
            CreateMap<UpdatePriceAlertRequest, PriceAlert>();

            CreateMap<ProductContract, ProductCacheModel>();

            CreateMap<BidAskPairRabbitMqContract, QuoteCacheModel>()
                .ForMember(x => x.ProductId,
                    opt => opt.MapFrom(x => x.Instrument))
                .ForMember(x => x.Timestamp,
                    opt => opt.MapFrom(x => x.Date));

            CreateMap<BestPriceContract, QuoteCacheModel>()
                .ForMember(x => x.ProductId,
                    opt => opt.MapFrom(x => x.Id));
        }
    }
}