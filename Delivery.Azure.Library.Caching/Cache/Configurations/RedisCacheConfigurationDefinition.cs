using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Definitions;

namespace Delivery.Azure.Library.Caching.Cache.Configurations
{
    public class RedisCacheConfigurationDefinition : SecureConfigurationDefinition
    {
        /// <summary>
        ///     The full connection string to the redis cache instance
        /// </summary>
        public virtual async Task<string> GetConnectionStringAsync()
        {
            return await SecretProvider.GetSecretAsync("RedisCache-ConnectionString");
        }

        /// <summary>
        ///     How long an item should stay in the cache
        /// </summary>
        public virtual int DefaultCacheExpirySeconds => ConfigurationProvider.GetSettingOrDefault<int>("RedisCache_DefaultExpirySeconds", 4 * 60 * 60);

        public RedisCacheConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}