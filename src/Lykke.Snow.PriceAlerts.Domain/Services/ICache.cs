using System.Threading.Tasks;

namespace Lykke.Snow.PriceAlerts.Domain.Services
{
    public interface ICache
    {
        Task Init();
    }
}