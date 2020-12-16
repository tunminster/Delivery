using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Configurations;
using Delivery.Azure.Library.Caching.Cache.Interfaces;
using Delivery.Azure.Library.Core.Monads;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Caching.Cache
{
    /// <summary>
    ///     A store which holds object data as a <see cref="MemoryCache" />
    ///     Dependencies:
    ///     <see cref="IConfigurationProvider" />
    ///     <see cref="IApplicationInsightsTelemetry" />
    ///     Settings:
    ///     <see cref="MemoryCacheConfigurationDefinition" />
    /// </summary>
    public class ManagedMemoryCache : IManagedCache
    {
        private readonly IServiceProvider serviceProvider;

        private readonly MemoryCacheConfigurationDefinition configurationDefinition;
        
        private readonly ConcurrentDictionary<string, MemoryCache> caches = new ConcurrentDictionary<string, MemoryCache>();

        public ManagedMemoryCache(IServiceProvider serviceProvider) : this(serviceProvider,
            new MemoryCacheConfigurationDefinition(serviceProvider))
        {
            
        }
        
        public ManagedMemoryCache(IServiceProvider serviceProvider, MemoryCacheConfigurationDefinition configurationDefinition)
        {
            this.serviceProvider = serviceProvider;
            this.configurationDefinition = configurationDefinition;
        }
        
        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        public virtual async Task<Maybe<T>> GetAsync<T>(string key, string partition)
        {
            if (!caches.ContainsKey(partition))
            {
                return Maybe<T>.NotPresent;
            }

            if (!caches.TryGetValue(partition, out var memoryCache))
            {
                return Maybe<T>.NotPresent;
            }

            var objectFromCache = memoryCache.Get(key);
            if (objectFromCache == default)
            {
                return Maybe<T>.NotPresent;
            }

            var value = ((Lazy<T>) objectFromCache).Value;

            await Task.CompletedTask;
            return new Maybe<T>(value);
        }

        public virtual async Task<Maybe<T>> AddAsync<T>(string key, T targetValue, string partition, string correlationId, int? cacheExpirySeconds = null)
        {
            var newValue = new Lazy<T>(() => targetValue);

            caches.AddOrUpdate(partition, CreateInitialCache(key, newValue, cacheExpirySeconds), (cacheKey, cacheValue) =>
            {
                cacheValue.Set(key, newValue, TimeSpan.FromSeconds(cacheExpirySeconds ?? configurationDefinition.CacheExpirySeconds));
                return cacheValue;
            });

            await Task.CompletedTask;
            return new Maybe<T>(targetValue);
        }
        
        private MemoryCache CreateInitialCache<TValue>(string key, TValue value, int? cacheExpirySeconds = null) where TValue : class
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set(key, value, TimeSpan.FromSeconds(cacheExpirySeconds ?? configurationDefinition.CacheExpirySeconds));

            return memoryCache;
        }

        public virtual async Task ClearAsync(string partition, string correlationId, string? key = null)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                await AddAsync(key, string.Empty, partition, correlationId);
            }
            else
            {
                ClearLocalMemoryRegion(partition);
                var customProperties = new Dictionary<string, string>
                {
                    {CustomProperties.CorrelationId, correlationId}
                };

                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"Cache partition {partition} cleared locally and from redis", SeverityLevel.Information, customProperties);
            }

            await Task.CompletedTask;
        }
        
        protected virtual void ClearLocalMemoryRegion(string partition)
        {
            if (caches.TryRemove(partition, out var deletionMemoryCache))
            {
                deletionMemoryCache.Dispose();
            }
        }
    }
}