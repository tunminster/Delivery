using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverHistory;
using Delivery.Driver.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverOrderHistory
{
    public record DriverOrderHistoryQuery
        (DateTimeOffset OrderDateFrom, DriverOrderStatus DriverOrderStatus) : IQuery<List<DriverOrderHistoryContract>>;
    
    
    public class DriverOrderHistoryQueryHandler : IQueryHandler<DriverOrderHistoryQuery, List<DriverOrderHistoryContract>>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverOrderHistoryQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<DriverOrderHistoryContract>> Handle(DriverOrderHistoryQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.DriverOrder>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();

            var cacheKey =
                $"Database-{executingRequestContextAdapter.GetShard().Key}-store-{query.DriverOrderStatus}-{nameof(DriverOrderHistoryQuery).ToLowerInvariant()}-{query.OrderDateFrom:dd-MM-yyyy-hh-mm-ss}";

            var driverHistoryContracts = await dataAccess.GetCachedItemsAsync(
                cacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.DriverOrders
                    .Where(x => x.InsertionDateTime >= query.OrderDateFrom
                                && x.Status == query.DriverOrderStatus)
                    .Include(x => x.Order)
                    .ThenInclude(x => x.Store)
                    .Select(x => x.ConvertToDriverHistoryContract()).ToListAsync()) ?? new List<DriverOrderHistoryContract>();
            
            return driverHistoryContracts;
        }
    }
}