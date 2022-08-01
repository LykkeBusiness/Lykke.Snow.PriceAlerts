using System;
using AutoMapper;
using Lykke.Snow.Common.Converters;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;
using Lykke.Snow.PriceAlerts.Domain.Models;

namespace Lykke.Snow.PriceAlerts.MappingProfiles
{
    public class PriceAlertsProfile : Profile
    {
        public PriceAlertsProfile()
        {
            CreateMap<DateTime, DateTime>().ConvertUsing<EnsureUtcDateTimeKindConverter>();

            CreateMap<PriceAlert, PriceAlertContract>();
            CreateMap<PriceAlertContract, PriceAlert>()
                .ForMember(x => x.CorrelationId,
                    opt => opt.Ignore());
        }
    }
}