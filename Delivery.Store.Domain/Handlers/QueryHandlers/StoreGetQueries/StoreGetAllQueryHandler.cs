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
    public class StoreGetAllQueryHandler : IQueryHandler<StoreGetAllQuery, List<StoreContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public StoreGetAllQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<StoreContract>> Handle(StoreGetAllQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.Store>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();

            var stores = await databaseContext.Stores
                .Include(x => x.StoreType)
                .Include(x => x.OpeningHours)
                .Include(x => x.StorePaymentAccount)
                .Where(x => !x.IsDeleted)
                .Skip(query.NumberOfObjectPerPage * (query.PageNumber - 1))
                .Take(query.NumberOfObjectPerPage).ToListAsync();
            
            if (stores == null)
            {
                return new List<StoreContract>();
            }
            
            var storeContractList = StoreContractConverter.Convert(stores);
            return storeContractList;
        }
    }
}