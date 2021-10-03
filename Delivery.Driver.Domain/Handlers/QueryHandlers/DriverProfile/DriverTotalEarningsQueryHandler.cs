using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverProfile
{
    public record DriverTotalEarningsQuery : IQuery<DriverTotalEarningsContract>
    {
        public string UserEmail { get; init; } = string.Empty;
    }
    public class DriverTotalEarningsQueryHandler : IQueryHandler<DriverTotalEarningsQuery, DriverTotalEarningsContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverTotalEarningsQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverTotalEarningsContract> Handle(DriverTotalEarningsQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            
            var cacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{query.UserEmail.ToLower()}-total-earning";
            var cache = await serviceProvider.GetInvalidationEnabledCacheAsync();

            if (cache != null)
            {
                var existingCachedItems = await cache.GetAsync<DriverTotalEarningsContract>(cacheKey, databaseContext.GlobalDatabaseCacheRegion);

                if (existingCachedItems.IsPresent)
                {
                    var customProperties = new Dictionary<string, string>
                    {
                        {CustomProperties.CorrelationId, databaseContext.ExecutingRequestContextAdapter.GetCorrelationId()}
                    };
                    
                    serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackMetric("Database Query Cache Hit", value: 1.0, customProperties);
                    return existingCachedItems.Value;
                }
            }
            
            var driver = await databaseContext.Drivers.FirstOrDefaultAsync(x => x.EmailAddress == query.UserEmail);
            var driverOrder = await databaseContext.DriverOrders
                .Include(x => x.Order)
                .Where(x => x.DriverId == driver.Id
                            && x.Status == DriverOrderStatus.Complete
                            && x.DriverPaymentStatus == DriverPaymentStatus.None)
                .ToListAsync();

            var totalOrder = driverOrder.Count;
            var totalEarnings = driverOrder.Select(x => x.Order.DeliveryFees).Sum();

            var driverTotalEarningsContract = new DriverTotalEarningsContract
            {
                TotalOrders = totalOrder,
                TotalEarnings = totalEarnings
            };

            if (cache != null)
            {
                await cache.AddAsync(cacheKey, driverTotalEarningsContract, databaseContext.GlobalDatabaseCacheRegion, databaseContext.ExecutingRequestContextAdapter.GetCorrelationId());
            }
            

            return driverTotalEarningsContract;
        }
    }
}