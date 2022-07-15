using JetBrains.Annotations;

namespace Lykke.Snow.PriceAlerts.Client
{
    /// <summary>
    /// LykkeService client interface.
    /// </summary>
    [PublicAPI]
    public interface IPriceAlertsClient
    {
        // Make your app's controller interfaces visible by adding corresponding properties here.
        // NO actual methods should be placed here (these go to controller interfaces, for example - ILykkeServiceApi).
        // ONLY properties for accessing controller interfaces are allowed.

        /// <summary>Application Api interface</summary>
        IPriceAlertsApi Api { get; }
    }
}
