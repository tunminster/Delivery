using System;
using Delivery.Azure.Library.Configuration.Configurations;

namespace Delivery.Azure.Library.KeyVault.Providers.Configurations
{
    public class KeyVaultCachedSecretConfigurationDefinition : KeyVaultSecretConfigurationDefinition
    {
        /// <summary>
        ///     The amount of time in seconds what the secret stays in the cache for
        /// </summary>
        public virtual int CacheExpiryInSeconds => ConfigurationProvider.GetSettingOrDefault<int>("KeyVault.Cache.ExpiryInSeconds", 30 * 24 * 60 * 60);

        public KeyVaultCachedSecretConfigurationDefinition(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}