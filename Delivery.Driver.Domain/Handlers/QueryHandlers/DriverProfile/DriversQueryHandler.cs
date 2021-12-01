using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Constants;
using Delivery.Driver.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverProfile
{
    public record DriversQuery(int PageNumber, int PageSize) : IQuery<DriversPageContract>;
    
    public class DriversQueryHandler : IQueryHandler<DriversQuery, DriversPageContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriversQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriversPageContract> Handle(DriversQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.DriverOrder>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            var driverCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{nameof(DriversQuery).ToLowerInvariant()}-{query.PageNumber}-{query.PageSize}";

            var driverTotal = await databaseContext.Drivers.CountAsync();
            
            var driverContracts = await dataAccess.GetCachedItemsAsync(
                driverCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.Drivers
                    .OrderByDescending(x => x.InsertionDateTime)
                    .Skip(query.PageSize * (query.PageNumber - 1))
                    .Take(query.PageSize).Select(x => x.ConvertToDriverContract()).ToListAsync());

            var driversPageContract = new DriversPageContract
            {
                TotalPages = (driverTotal + query.PageSize - 1) / query.PageSize,
                Data = driverContracts
            };

            return driversPageContract;
        }
    }
}