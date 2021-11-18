using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Contracts.V1.RestContracts;
using Delivery.Store.Domain.Converters.StoreConverters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries
{
    public class StoreGetAllQueryHandler : IQueryHandler<StoreGetAllQuery, StorePagedContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public StoreGetAllQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StorePagedContract> Handle(StoreGetAllQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.Store>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            
            var storeCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{query.PageSize}-{query.PageNumber}-{nameof(StoreGetAllQueryHandler).ToLowerInvariant()}";

            var storeTotal = await databaseContext.Stores.CountAsync();

            var storeContractList = await dataAccess.GetCachedItemsAsync(
                storeCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.Stores
                    .Include(x => x.StoreType)
                    .Include(x => x.OpeningHours)
                    .Include(x => x.StorePaymentAccount)
                    .Where(x => !x.IsDeleted)
                    .Skip(query.PageSize * (query.PageNumber - 1))
                    .Take(query.PageSize).Select(x => x.ConvertStoreContract()).ToListAsync());

            var storePageContract = new StorePagedContract
            {
                TotalPages = (storeTotal + query.PageSize - 1) / query.PageSize,
                Data = storeContractList
            };

            return storePageContract;
        }
    }
}