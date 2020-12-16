using System;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;

namespace Delivery.Azure.Library.Caching.Cache.Configurations
{
    public class MemoryCacheConfigurationDefinition : ConfigurationDefinition
    {
        /// <summary>
        ///     The number of seconds which the cache item lives for
        /// </summary>
        public virtual int CacheExpirySeconds => ConfigurationProvider.GetSettingOrDefault<int>("MemoryCache_DefaultExpirySeconds", 7 * 24 * 60 * 60);
        
        public MemoryCacheConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}