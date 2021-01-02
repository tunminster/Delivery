using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Configurations;
using Delivery.Azure.Library.Caching.Cache.FeatureFlags;
using Delivery.Azure.Library.Caching.Cache.Interfaces;
using Delivery.Azure.Library.Configuration.Features.Interfaces;
using Delivery.Azure.Library.Core.Monads;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Caching.Cache
{
    public class ManagedDistributedMemoryCache : ManagedMemoryCache
    {
        private readonly IServiceProvider serviceProvider;
		private readonly IManagedCache invalidationCache;
		private readonly string cacheRegionInvalidationKey = "CacheRegionInvalidation";
		private readonly ConcurrentDictionary<string, long> cacheRegionChanges = new();

		public ManagedDistributedMemoryCache(IServiceProvider serviceProvider) : this(serviceProvider, new MemoryCacheConfigurationDefinition(serviceProvider), new RedisCacheConfigurationDefinition(serviceProvider))
		{
		}

		public ManagedDistributedMemoryCache(IServiceProvider serviceProvider, MemoryCacheConfigurationDefinition configurationDefinition, RedisCacheConfigurationDefinition redisCacheConfigurationDefinition) : base(serviceProvider, configurationDefinition)
		{
			this.serviceProvider = serviceProvider;
			invalidationCache = new ManagedRedisCache(serviceProvider, redisCacheConfigurationDefinition);
		}

		public override async Task ClearAsync(string partition, string correlationId, string? key = null)
		{
			if (string.IsNullOrEmpty(key))
			{
				var redisRegionKey = CreateRedisCacheInvalidationKey(partition);
				await invalidationCache.ClearAsync(redisRegionKey, correlationId);
				await invalidationCache.AddAsync(redisRegionKey, DateTimeOffset.UtcNow.Ticks, partition, correlationId);
			}

			await base.ClearAsync(partition, correlationId, key);
		}

		private string CreateRedisCacheInvalidationKey(string partition)
		{
			return $"{partition}_{cacheRegionInvalidationKey}";
		}

		public override async Task<Maybe<T>> GetAsync<T>(string key, string partition)
		{
			await ClearMemoryCacheOnInvalidationChangeAsync(partition);

			return await base.GetAsync<T>(key, partition);
		}

		public override async Task<Maybe<T>> AddAsync<T>(string key, T targetValue, string partition, string correlationId, int? cacheExpirySeconds = null)
		{
			await ClearMemoryCacheOnInvalidationChangeAsync(partition);
			return await base.AddAsync(key, targetValue, partition, correlationId, cacheExpirySeconds);
		}

		private async Task ClearMemoryCacheOnInvalidationChangeAsync(string partition)
		{
			var featureProvider = serviceProvider.GetRequiredService<IFeatureProvider>();
			if (await featureProvider.IsEnabledAsync(CachingFeatures.RedisCache.ToString()))
			{
				var cacheRegionValue = await invalidationCache.GetAsync<long?>(CreateRedisCacheInvalidationKey(partition), partition);
				if (cacheRegionValue.IsPresent)
				{
					var firstCacheInvalidationCheck = !cacheRegionChanges.ContainsKey(partition);
					var cacheRegionChange = cacheRegionValue.Value.GetValueOrDefault();
					var cacheIsInvalidated = cacheRegionChanges.ContainsKey(partition) && cacheRegionChanges[partition] < cacheRegionChange;

					if (firstCacheInvalidationCheck || cacheIsInvalidated)
					{
						ClearLocalMemoryRegion(partition);
					}

					if (cacheIsInvalidated)
					{
						var customProperties = new Dictionary<string, string>
						{
							{
								"CacheLastModifiedLocal", new DateTime(cacheRegionChanges[partition], DateTimeKind.Utc).ToString("o")
							},
							{
								"CacheLastModifiedRedis", new DateTime(cacheRegionChange, DateTimeKind.Utc).ToString("o")
							}
						};

						serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Cache partition {partition} cleared in local memory only", SeverityLevel.Information, customProperties);
					}

					cacheRegionChanges[partition] = cacheRegionChange;
				}
			}
		}

		public override async ValueTask DisposeAsync()
		{
			await invalidationCache.DisposeAsync();
			await base.DisposeAsync();
		}
    }
}