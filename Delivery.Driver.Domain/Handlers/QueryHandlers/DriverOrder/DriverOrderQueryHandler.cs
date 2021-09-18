using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;
using Delivery.Driver.Domain.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverOrder
{
    public record DriverOrderQuery : IQuery<DriverOrderDetailsContract>;
        
    public class DriverOrderQueryHandler : IQueryHandler<DriverOrderQuery, DriverOrderDetailsContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverOrderQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverOrderDetailsContract> Handle(DriverOrderQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected an authenticated user.");

            var cacheKey = $"{executingRequestContextAdapter.GetShard().Key.ToLower()}-{userEmail.ToLower()}";
            var cache = await serviceProvider.GetInvalidationEnabledCacheAsync();
            var driverId = 0;
            if (cache != null)
            {
                var existingCachedItems = await cache.GetAsync<Database.Entities.Driver>(cacheKey, databaseContext.GlobalDatabaseCacheRegion);
                if (existingCachedItems.IsPresent)
                {
                    var customProperties = new Dictionary<string, string>
                    {
                        {CustomProperties.CorrelationId, databaseContext.ExecutingRequestContextAdapter.GetCorrelationId()}
                    };

                    serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Database Query Cache Hit", value: 1.0, customProperties);
                    driverId = existingCachedItems.Value.Id;
                }
                else
                {
                    var driver = await databaseContext.Drivers.SingleOrDefaultAsync(x => x.EmailAddress == userEmail);
                    driverId = driver.Id;
                }
            }
            
            var driverOrders = await databaseContext.DriverOrders
                .Include(x => x.Order.Store)
                .Include(x => x.Order)
                .ThenInclude(x => x.Address)
                .FirstOrDefaultAsync(x => x.DriverId == driverId && x.Status == DriverOrderStatus.None);

            if (driverOrders == null)
            {
                return new DriverOrderDetailsContract();
            }
            
            var driverOrderDetailsContract = driverOrders.ConvertToDriverOrderDetailsContract();

            return driverOrderDetailsContract;
        }
    }
}