using System;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;

namespace Delivery.Azure.Library.Resiliency.Stability.Configurations
{
    public class CircuitBreakerConfigurationDefinition : ConfigurationDefinition
    {
        private int? cachedDefaultRetryLimit;

        /// <summary>
        ///     The number of times to retry before giving up
        /// </summary>
        public virtual int DefaultRetryLimit
        {
            get { return cachedDefaultRetryLimit ??= ConfigurationProvider.GetSettingOrDefault<int>("CircuitBreaker_RetryDefaultLimit", defaultValue: 4); }
        }

        public CircuitBreakerConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}