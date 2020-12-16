using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache;
using Delivery.Azure.Library.Caching.Cache.Interfaces;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.KeyVault.Providers.Configurations;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;

namespace Delivery.Azure.Library.KeyVault.Providers
{
    /// <summary>
	///     Manages access to secrets stored in key vault and provides a managed cache layer
	/// </summary>
	/// Dependencies:
	/// <see cref="IConfigurationProvider" />
	/// <see cref="IApplicationInsightsTelemetry" />
	/// <see cref="IManagedCache" />
	/// Settings:
	/// <see cref="CachedConfigurationDefinition" />
	public class KeyVaultCachedSecretProvider : KeyVaultSecretProvider, ICachedSecretProvider
	{
		private KeyVaultCachedSecretConfigurationDefinition CachedConfigurationDefinition => (KeyVaultCachedSecretConfigurationDefinition) ConfigurationDefinition;
		private readonly IManagedCache cache;

		public KeyVaultCachedSecretProvider(IServiceProvider serviceProvider) : base(serviceProvider, new KeyVaultCachedSecretConfigurationDefinition(serviceProvider))
		{
			cache = new ManagedMemoryCache(serviceProvider);
		}

		public KeyVaultCachedSecretProvider(IServiceProvider serviceProvider, KeyVaultCachedSecretConfigurationDefinition configurationDefinition) : base(serviceProvider, configurationDefinition)
		{
			cache = new ManagedMemoryCache(serviceProvider);
		}

		public override async Task<string> GetSecretAsync(string secretName, string? shardKey = null)
		{
			// Fetch from cache, when applicable
			var cachedEntry = await cache.GetAsync<string>(secretName, ConfigurationDefinition.VaultUri);
			if (cachedEntry.IsPresent)
			{
				return cachedEntry.Value;
			}

			// Fetch secret from the vault
			var secretValue = await base.GetSecretAsync(secretName, shardKey);

			// Add secret to cache
			await cache.AddAsync(secretName, secretValue, ConfigurationDefinition.VaultUri, string.Empty, CachedConfigurationDefinition.CacheExpiryInSeconds);

			return secretValue;
		}

		public async Task ClearCacheAsync(string? secretName = null)
		{
			if (secretName != null)
			{
				await cache.AddAsync(secretName, string.Empty, ConfigurationDefinition.VaultUri, string.Empty, CachedConfigurationDefinition.CacheExpiryInSeconds);
			}
			else
			{
				await cache.ClearAsync(ConfigurationDefinition.VaultUri, string.Empty);
			}
		}
	}
}