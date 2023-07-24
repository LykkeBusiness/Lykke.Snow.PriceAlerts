using Lykke.Contracts.Messaging;
using Lykke.Snow.PriceAlerts.Contract.Models.Contracts;
using MessagePack;

namespace Lykke.Snow.PriceAlerts.Contract.Models.Events
{
    [MessagePackObject]
    public class PriceAlertChangedEvent : EntityChangedEvent<PriceAlertContract, PriceAlertContextContract>
    {
    }
}