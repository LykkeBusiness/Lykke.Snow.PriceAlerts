using System;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Snow.PriceAlerts.Settings
{
    public class CqrsSettings
    {
        [AmqpCheck] public string ConnectionString { get; set; }

        public TimeSpan RetryDelay { get; set; }

        [Optional] [CanBeNull] public string EnvironmentName { get; set; }

        [Optional] public CqrsContextNamesSettings ContextNames { get; set; } = new CqrsContextNamesSettings();
    }
}