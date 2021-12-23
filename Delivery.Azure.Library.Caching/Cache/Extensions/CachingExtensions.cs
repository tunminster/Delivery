using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Configurations;
using Delivery.Azure.Library.Caching.Cache.FeatureFlags;
using Delivery.Azure.Library.Caching.Cache.Interfaces;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Features.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using ServiceStack.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.System.Text.Json;

namespace Delivery.Azure.Library.Caching.Cache.Extensions
{
    public static class CachingExtensions
    {
        public static IServiceCollection AddPlatformCaching(this IServiceCollection serviceCollection)
        {
            var connectionString = new RedisCacheConfigurationDefinition(serviceCollection.BuildServiceProvider()).GetConnectionString();
            
            serviceCollection.AddSingleton<IRedisClientsManagerAsync>(_ => new RedisManagerPool(connectionString));
            
            return serviceCollection;
        }

        public static IServiceCollection AddPlatformStackExchangeCaching(this IServiceCollection serviceCollection)
        {
            var connectionString = new RedisCacheConfigurationDefinition(serviceCollection.BuildServiceProvider()).GetConnectionStackExchangeRedis();
            serviceCollection.AddStackExchangeRedisExtensions<SystemTextJsonSerializer>(new RedisConfiguration
            {
                ConnectionString = connectionString
            });
            return serviceCollection;
        }

        /// <summary>
        ///     Returns an instance of the cache if the cache is able to be invalidated with calls to ClearAsync, otherwise returns
        ///     null
        /// </summary>
        public static async Task<IManagedCache> GetInvalidationEnabledCacheAsync(this IServiceProvider serviceProvider, bool alwaysUseCache = false)
        {
            var cache = serviceProvider.GetService<IManagedCache?>();
            var featureProvider = serviceProvider.GetRequiredService<IFeatureProvider>();
            if (!await featureProvider.IsEnabledAsync(CachingFeatures.MutableCache.ToString()))
            {
                return null;
            }

            if (alwaysUseCache)
            {
                return cache;
            }

            var useInMemory = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault<bool>("Test_Use_In_Memory", defaultValue: false);
            return useInMemory ? null : cache;
        }
    }
}