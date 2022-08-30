using AutoMapper;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.SqlRepositories.Entities;

namespace Lykke.Snow.PriceAlerts.SqlRepositories.MappingProfiles
{
    public class StorageMappingProfile : Profile
    {
        public StorageMappingProfile()
        {
            // Price alerts
            CreateMap<PriceAlert, PriceAlertEntity>().ReverseMap();
        }
    }
}