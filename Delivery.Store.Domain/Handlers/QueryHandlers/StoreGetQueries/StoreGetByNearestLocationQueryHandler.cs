using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Converters.StoreConverters;
using Microsoft.EntityFrameworkCore;


namespace Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries
{
    public class StoreGetByStoreTypeIdAndGeoLocationQuery : IQueryHandler<StoreGetByNearestLocationQuery, List<StoreContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public StoreGetByStoreTypeIdAndGeoLocationQuery(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<StoreContract>> Handle(StoreGetByNearestLocationQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.Store>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            
            var storeContractList = await dataAccess.GetCachedItemsAsync(query.CacheKey, databaseContext.GlobalDatabaseCacheRegion,
                async () =>
                    await (from st in databaseContext.Stores
                            .Include(x => x.StoreType)
                        let distance = Math.Sqrt(Math.Pow(69.1 * (st.Latitude ?? 0 - query.Latitude), 2) +
                                                 Math.Pow(
                                                     69.1 * (query.Longitude - st.Longitude ?? 0) *
                                                     Math.Cos(st.Latitude ?? 0 / 57.3), 2))
                        where distance < query.Distance && !st.IsDeleted
                        select new StoreContract()
                        {
                            StoreId = st.ExternalId,
                            StoreName = st.StoreName,
                            AddressLine1 = st.AddressLine1,
                            AddressLine2 = st.AddressLine2,
                            City = st.City,
                            County = st.County,
                            Country = st.Country,
                            PostalCode = st.PostalCode,
                            StoreType = st.StoreType.StoreTypeName,
                            Distance = distance
                        })
                    .Skip(query.NumberOfObjectPerPage * (query.PageNumber - 1))
                    .Take(query.NumberOfObjectPerPage).ToListAsync());

            if (storeContractList == null)
            {
                return new List<StoreContract>();
            }
            
            return storeContractList;
            
        }
    }
}