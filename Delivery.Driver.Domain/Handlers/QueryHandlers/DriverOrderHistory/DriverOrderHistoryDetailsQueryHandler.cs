using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverHistory;
using Delivery.Driver.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverOrderHistory
{
    public record DriverOrderHistoryDetailsQuery(string OrderId) : IQuery<DriverOrderHistoryDetailsContract>;
    public class DriverOrderHistoryDetailsQueryHandler : IQueryHandler<DriverOrderHistoryDetailsQuery,DriverOrderHistoryDetailsContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverOrderHistoryDetailsQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverOrderHistoryDetailsContract> Handle(DriverOrderHistoryDetailsQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.DriverOrder>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            
            var driverCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-driver-id-{executingRequestContextAdapter.GetAuthenticatedUser().UserEmail!.ToLowerInvariant()}";

            var driver = await dataAccess.GetCachedItemsAsync(
                driverCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.Drivers.SingleOrDefaultAsync(x =>
                    x.EmailAddress == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail)) ?? throw new InvalidOperationException($"Expected to be found driver.");
            
            var cacheKey =
                $"Database-{executingRequestContextAdapter.GetShard().Key}-order-history-{query.OrderId}";
            
            var driverHistoryDetailsContract = await dataAccess.GetCachedItemsAsync(
                cacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.DriverOrders
                    .Where(x => x.DriverId >= driver.Id && x.Order.ExternalId == query.OrderId)
                    .Include(x => x.Order)
                    .ThenInclude(x => x.Store)
                    .Include(x => x.Order.OrderItems)
                    .ThenInclude(x => x.Product)
                    .Select(x => x.ConvertToDriverOrderHistoryDetailsContract()).FirstOrDefaultAsync());

            return driverHistoryDetailsContract ?? throw new FileNotFoundException($"Expected to be found and order. Instead: {driverHistoryDetailsContract.ConvertToJson()}");
        }
    }
}