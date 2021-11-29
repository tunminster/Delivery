using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.RestContracts;
using Delivery.Store.Domain.Converters.StoreConverters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries
{
    public record StoreManagementGetAllQuery(int PageSize, int PageNumber) : IQuery<StoreManagementPagedContract>;
    public class StoreManagementGetAllQueryHandler : IQueryHandler<StoreManagementGetAllQuery, StoreManagementPagedContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public StoreManagementGetAllQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StoreManagementPagedContract> Handle(StoreManagementGetAllQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.Store>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            
            var storeCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{query.PageSize}-{query.PageNumber}-{nameof(StoreManagementGetAllQuery).ToLowerInvariant()}";

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
                    .Take(query.PageSize).Select(x => x.ConvertStoreManagementContract()).ToListAsync());

            var storeManagementPagedContract = new StoreManagementPagedContract
            {
                TotalPages = (storeTotal + query.PageSize - 1) / query.PageSize,
                Data = storeContractList
            };

            return storeManagementPagedContract;
        }
    }
}