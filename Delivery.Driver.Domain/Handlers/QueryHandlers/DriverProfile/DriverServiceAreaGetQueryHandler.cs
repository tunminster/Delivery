using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using Delivery.Driver.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverProfile
{
    public record DriverServiceAreaGetQuery(string DriverEmail) : IQuery<DriverServiceAreaContract>;
    public class DriverServiceAreaGetQueryHandler : IQueryHandler<DriverServiceAreaGetQuery, DriverServiceAreaContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverServiceAreaGetQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverServiceAreaContract> Handle(DriverServiceAreaGetQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.DriverOrder>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();

            var driverEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail?.ToLowerInvariant() ??
                              throw new InvalidOperationException("Expected a driver");
            
            var driverCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{nameof(DriverServiceAreaGetQuery).ToLowerInvariant()}-{driverEmail}";
            
            var driverServiceAreaContract = await dataAccess.GetCachedItemsAsync(
                driverCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.Drivers.Where(x =>
                    x.EmailAddress == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail).Select(x => x.ConvertToDriverServiceAreaContract()).SingleOrDefaultAsync()) ?? throw new InvalidOperationException($"Expected to be found driver.");
            
            return driverServiceAreaContract;
        }
    }
}