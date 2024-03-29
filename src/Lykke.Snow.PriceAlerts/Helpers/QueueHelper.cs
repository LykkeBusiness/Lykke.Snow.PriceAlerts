using Microsoft.Extensions.PlatformAbstractions;

namespace Lykke.Snow.PriceAlerts.Helpers
{
    public static class QueueHelper
    {
        public static string BuildQueueName(string exchangeName, string env, string postfix = "")
        {
            return
                $"{exchangeName}.{PlatformServices.Default.Application.ApplicationName}.{env ?? "DefaultEnv"}{postfix}";
        }
    }
}