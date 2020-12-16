using System;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;

namespace Delivery.Azure.Library.Resiliency.Stability.Configurations
{
    public class CircuitManagerConfigurationDefinition : ConfigurationDefinition
    {
        private int? cachedHandledEventsBeforeBreaking;

        /// <summary>
        ///     The number of events handled before the circuit is closed
        /// </summary>
        public virtual int HandledEventsBeforeBreaking
        {
            get { return cachedHandledEventsBeforeBreaking ??= ConfigurationProvider.GetSettingOrDefault<int>("CircuitManager_HandledEventsBeforeBreaking", defaultValue: 10); }
        }

        public CircuitManagerConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}