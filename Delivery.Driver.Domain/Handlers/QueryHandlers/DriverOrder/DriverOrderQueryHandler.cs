using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Caching.Cache.Extensions;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Azure.Library.Telemetry.Constants;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;
using Delivery.Driver.Domain.Converters;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverOrder
{
    public record DriverOrderQuery : IQuery<DriverOrderDetailsContract?>;
        
    public class DriverOrderQueryHandler : IQueryHandler<DriverOrderQuery, DriverOrderDetailsContract?>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverOrderQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverOrderDetailsContract?> Handle(DriverOrderQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected an authenticated user.");
            
            var driver = await databaseContext.Drivers.SingleOrDefaultAsync(x => x.EmailAddress == userEmail);
            
            var driverOrders = await databaseContext.DriverOrders
                .Include(x => x.Order.Store)
                .Include(x => x.Order.OrderItems)
                .ThenInclude(x => x.Product)
                .Include(x => x.Order.Customer)
                .Include(x => x.Order)
                .ThenInclude(x => x.Address)
                .FirstOrDefaultAsync(x => x.DriverId == driver.Id && (x.Status == DriverOrderStatus.None || x.Status == DriverOrderStatus.Accepted || x.Status == DriverOrderStatus.InProgress));

            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(DriverOrderQueryHandler)} returns {driverOrders.ConvertToJson()}", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
            if (driverOrders == null)
            {
                return null;
            }
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(DriverOrderQueryHandler)} is going to convert", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
            
            var driverOrderDetailsContract = driverOrders.ConvertToDriverOrderDetailsContract();
            
            return driverOrderDetailsContract;
        }
    }
}