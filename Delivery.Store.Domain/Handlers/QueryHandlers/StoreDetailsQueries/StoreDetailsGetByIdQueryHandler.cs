using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.Contracts;
using Delivery.Product.Domain.Contracts.V1.ModelContracts;
using Delivery.Product.Domain.Converters;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Store.Domain.Handlers.QueryHandlers.StoreDetailsQueries
{
    public class StoreDetailsGetByIdQueryHandler : IQueryHandler<StoreDetailsGetByIdQuery, StoreDetailsContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public StoreDetailsGetByIdQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StoreDetailsContract> Handle(StoreDetailsGetByIdQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var cache = await serviceProvider.GetInvalidationEnabledCacheAsync();

            if (cache != null)
            {
                var existingCachedItems = await cache.GetAsync<StoreDetailsContract>(query.CacheKey, databaseContext.GlobalDatabaseCacheRegion);
                if (existingCachedItems.IsPresent)
                {
                    var customProperties = new Dictionary<string, string>
                    {
                        {CustomProperties.CorrelationId, databaseContext.ExecutingRequestContextAdapter.GetCorrelationId()}
                    };

                    serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Database Query Cache Hit", value: 1.0, customProperties);
                    return existingCachedItems.Value;
                }
            }
            
            var store = await databaseContext.Stores.FirstOrDefaultAsync(x => x.ExternalId == query.StoreId);

            var products = await databaseContext.Products.Where(x => x.StoreId == store.Id)
                .Include(x => x.Category)
                .Include(x => x.MeatOptions)
                .ThenInclude(x => x.MeatOptionValues)
                .ToListAsync();
            var categories = products.Select(x => x.Category).Distinct().ToList();

            var storeCategoriesContractList = new List<StoreCategoriesContract>();
            foreach (var category in categories)
            {
                storeCategoriesContractList.Add(new StoreCategoriesContract
                {
                    Id = category.ExternalId,
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    ParentCategoryId = category.ParentCategoryId,
                    Products = products.Where(x => x.CategoryId == category.Id).Select(p => new ProductContract
                    {
                        Id = p.ExternalId,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        ProductImage = p.ProductImage,
                        ProductImageUrl = p.ProductImageUrl,
                        CategoryId = p.Category.ExternalId,
                        CategoryName = p.Category.CategoryName,
                        StoreId = store.ExternalId,
                        UnitPrice = p.UnitPrice,
                        ProductMeatOptions = p.MeatOptions.Select(x => x.ConvertToProductMeatOptionContract()).ToList()
                    }).ToList()
                });
            }

            var storeDetailContract = new StoreDetailsContract
            {
                StoreId = store.ExternalId,
                StoreName = store.StoreName,
                AddressLine1 = store.AddressLine1,
                AddressLine2 = store.AddressLine2,
                StoreCategoriesList = storeCategoriesContractList,
                ImageUri = store.ImageUri,
                City = store.City,
                Country = store.Country,
                PostalCode = store.PostalCode
            };
            
            if (cache == null)
            {
                return storeDetailContract;
            }
            
            await cache.AddAsync(query.CacheKey, storeDetailContract, databaseContext.GlobalDatabaseCacheRegion, databaseContext.ExecutingRequestContextAdapter.GetCorrelationId());

            return storeDetailContract;
        }
    }
}