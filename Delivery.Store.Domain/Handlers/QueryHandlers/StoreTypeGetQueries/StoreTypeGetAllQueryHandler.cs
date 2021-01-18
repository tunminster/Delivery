using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Converters.StoreTypeConverters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Store.Domain.Handlers.QueryHandlers.StoreTypeGetQueries
{
    public class StoreTypeGetAllQueryHandler : IQueryHandler<StoreTypeGetAllQuery, List<StoreTypeContract>>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public StoreTypeGetAllQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<StoreTypeContract>> Handle(StoreTypeGetAllQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.StoreType>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));

            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            var storeTypes = await dataAccess.GetCachedItemsAsync(query.CacheKey, databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.StoreTypes.Where(x => !x.IsDeleted).ToListAsync());

            if (storeTypes == null)
            {
                return new List<StoreTypeContract>();
            }

            var storeTypeContractList = StoreTypeContractConverter.Convert(storeTypes);

            return storeTypeContractList;
        }
    }
}