using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.FeatureFlags;
using Delivery.Azure.Library.Caching.Cache.Interfaces;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Configuration.Features.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.Caching.Cache.Extensions
{
    public static class CachingExtensions
    {
        public static IServiceCollection AddPlatformRedisCache(this IServiceCollection serviceCollection)
        {
            return new JoinableTaskContext().Factory.Run(async () => await AddPlatformRedisCacheAsync(serviceCollection));
        }

        public static async Task<IServiceCollection> AddPlatformRedisCacheAsync(this IServiceCollection serviceCollection)
        {
            var connectionString = await new RedisCacheConfigurationDefinition(serviceCollection.BuildServiceProvider()).GetConnectionStringAsync();

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
        public static async Task<IManagedCache?> GetInvalidationEnabledCacheAsync(this IServiceProvider serviceProvider, bool alwaysUseCache = false)
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