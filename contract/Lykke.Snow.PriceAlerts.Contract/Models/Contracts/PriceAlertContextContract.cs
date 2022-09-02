using MessagePack;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Contracts
{
    [MessagePackObject()]
    public class PriceAlertContextContract
    {
        [Key(0)]
        public bool HasActiveAlerts { get; set; }
    }
}