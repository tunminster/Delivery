using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverOrderRejection;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrderRejection;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverReAssignment;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverIndex;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverRequest;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Strategies.DriverAssignmentStrategy
{
    public class ReAssignDeliveryOrderStrategy : DriverReAssignmentStrategy
    {
        public ReAssignDeliveryOrderStrategy(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public override bool AppliesTo(DriverOrderStatus driverOrderStatus) => driverOrderStatus.Equals(DriverOrderStatus.None) || driverOrderStatus.Equals(DriverOrderStatus.Accepted) || driverOrderStatus.Equals(DriverOrderStatus.Rejected) || driverOrderStatus.Equals(DriverOrderStatus.SystemRejected);
        

        public override async Task<DriverReAssignmentCreationStatusContract> ExecuteAsync(int driverOrderId, int awaitingMinutes)
        {
            await using var databaseContext =
                await PlatformDbContext.CreateAsync(ServiceProvider, ExecutingRequestContextAdapter);

            var driverOrder = await databaseContext.DriverOrders
                .Where(x => x.Id == driverOrderId)
                .Include(x => x.Driver)
                .Include(x => x.Order).FirstAsync();
            
            var dateTimeDiff = driverOrder.InsertionDateTime.Subtract(DateTimeOffset.UtcNow);
            var totalDifferentMinute = dateTimeDiff.TotalMinutes;
            
            ServiceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"Awaited time to accept the order {totalDifferentMinute}. OrderId: {driverOrder.Order.ExternalId}", SeverityLevel.Information, ExecutingRequestContextAdapter.GetTelemetryProperties());
            
            driverOrder.Status = DriverOrderStatus.SystemRejected;
            driverOrder.Reason = "System rejected";

            driverOrder.Driver.IsOrderAssigned = false;
            await databaseContext.SaveChangesAsync();
            
            // re-index driver after rejection 
            await new DriverIndexCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                .HandleAsync(new DriverIndexCommand(driverOrder.Driver.ExternalId));

            var driverRequestMessageContract = new DriverRequestMessageContract
            {
                PayloadIn = new DriverRequestContract {OrderId = driverOrder.Order.ExternalId},
                PayloadOut = new StatusContract {Status = true, DateCreated = DateTimeOffset.UtcNow},
                RequestContext = ExecutingRequestContextAdapter.GetExecutingRequestContext()
            };

            // request another driver
            await new DriverRequestMessagePublisher(ServiceProvider, ExecutingRequestContextAdapter).ExecuteAsync(
                driverRequestMessageContract);

            return new DriverReAssignmentCreationStatusContract
            {
                DriverId = driverOrder.Driver.ExternalId,
                OrderId = driverOrder.Order.ExternalId, AssignedDateTime = DateTimeOffset.Now,
                DriverOrderStatus = driverOrder.Status
            };
        }
    }
}