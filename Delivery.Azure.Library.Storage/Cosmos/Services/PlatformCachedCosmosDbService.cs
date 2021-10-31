using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Cosmos.Contracts;
using Delivery.Azure.Library.Storage.Cosmos.Interfaces;
using Microsoft.Azure.Cosmos;

namespace Delivery.Azure.Library.Storage.Cosmos.Services
{
    public class PlatformCachedCosmosDbService : IPlatformCachedCosmosDbService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        private readonly PlatformCosmosDbService platformCosmosDbService;
        
        public PlatformCachedCosmosDbService(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter,
            PlatformCosmosDbService platformCosmosDbService)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
            this.platformCosmosDbService = platformCosmosDbService;
        }
        
        public async Task AddItemAsync<TDocument, TContract>(TDocument item) where TDocument : DocumentContract<TContract>, new() where TContract : class
        {
            var memoryCachePartition = typeof(TContract).Name;
            await platformCosmosDbService.AddItemAsync<TDocument, TContract>(item);
            var cache = await serviceProvider.GetInvalidationEnabledCacheAsync(alwaysUseCache: true);
            if (cache != null)
            {
                await cache.ClearAsync(memoryCachePartition, executingRequestContextAdapter.GetCorrelationId());
            }
        }

        public async Task DeleteItemAsync<TDocument, TContract>(string id, string partitionKey, bool throwIfNotFound = true) where TDocument : DocumentContract<TContract>, new() where TContract : class
        {
            var memoryCachePartition = typeof(TContract).Name;
            await platformCosmosDbService.DeleteItemAsync<TDocument, TContract>(id, partitionKey, throwIfNotFound);
            var cache = await serviceProvider.GetInvalidationEnabledCacheAsync(alwaysUseCache: true);
            if (cache != null)
            {
                await cache.ClearAsync(memoryCachePartition, executingRequestContextAdapter.GetCorrelationId());
            }
        }

        public async Task<TDocument?> GetLatestDocumentAsync<TDocument, TContract>(string partitionKey, bool isDocumentRequired = true,
            DateTimeOffset? validFromDate = null, DateTimeOffset? validToDate = null) where TDocument : DocumentContract<TContract>, new() where TContract : class
        {
            const double defaultMinutesToCache = 10;
            return await GetLatestDocumentAsync<TDocument, TContract>(partitionKey, defaultMinutesToCache, isDocumentRequired, validFromDate, validToDate);
        }

        public async Task<TContract?> GetItemAsync<TContract>(string partitionKey, Guid id) where TContract : class
        {
            var memoryCachePartition = typeof(TContract).Name;
            var cacheKey = $"{partitionKey}-{id}";
            var cache = await serviceProvider.GetInvalidationEnabledCacheAsync(alwaysUseCache: true);
            if (cache != null)
            {
                var cachedItem = await cache.GetAsync<TContract>(cacheKey, memoryCachePartition);
                if (cachedItem.IsPresent)
                {
                    return cachedItem.Value;
                }
            }

            var contract = await platformCosmosDbService.GetItemAsync<TContract>(partitionKey, id);
            var cacheExpiryTimespanSeconds = (int) TimeSpan.FromMinutes(value: 10).TotalSeconds;
            if (cache != null && contract != null)
            {
                await cache.AddAsync(cacheKey, contract, memoryCachePartition, executingRequestContextAdapter.GetCorrelationId(), cacheExpiryTimespanSeconds);
            }

            return contract;
        }

        public async Task<IEnumerable<TContract?>> GetItemsAsync<TContract>(QueryDefinition queryDefinition, QueryRequestOptions? queryRequestOptions = null,
            string? executedQuery = null) where TContract : class
        {
            return await platformCosmosDbService.GetItemsAsync<TContract>(queryDefinition, queryRequestOptions, executedQuery);
        }

        public async Task UpdateItemAsync<TDocument, TContract>(string partitionKey, TContract item) where TDocument : DocumentContract<TContract>, new() where TContract : class
        {
            var memoryCachePartition = typeof(TContract).Name;
            await platformCosmosDbService.UpdateItemAsync<TDocument, TContract>(partitionKey, item);
            var cache = await serviceProvider.GetInvalidationEnabledCacheAsync(alwaysUseCache: true);
            if (cache != null)
            {
                await cache.ClearAsync(memoryCachePartition, executingRequestContextAdapter.GetCorrelationId());
            }
        }

        public async Task<TDocument?> GetLatestDocumentAsync<TDocument, TContract>(string partitionKey, double minutesToCache,
            bool isDocumentRequired = true, DateTimeOffset? validFromDate = null, DateTimeOffset? validToDate = null) where TDocument : DocumentContract<TContract>, new() where TContract : class
        {
            var memoryCachePartition = typeof(TContract).Name;
            
            var cacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{partitionKey}-latest-document";
            var cache = await serviceProvider.GetInvalidationEnabledCacheAsync(alwaysUseCache: true);
            if (cache != null)
            {
                var cachedItem = await cache.GetAsync<TDocument>(cacheKey, memoryCachePartition);
                if (cachedItem.IsPresent)
                {
                    return cachedItem.Value;
                }
            }

            var contract = await platformCosmosDbService.GetLatestDocumentAsync<TDocument, TContract>(partitionKey, isDocumentRequired, validFromDate, validToDate);
            var cacheExpiryTimespanSeconds = (int) TimeSpan.FromMinutes(value: minutesToCache).TotalSeconds;
            if (cache != null && contract != null)
            {
                await cache.AddAsync(cacheKey, contract, memoryCachePartition, executingRequestContextAdapter.GetCorrelationId(), cacheExpiryTimespanSeconds);
            }

            return contract;
        }
    }
}