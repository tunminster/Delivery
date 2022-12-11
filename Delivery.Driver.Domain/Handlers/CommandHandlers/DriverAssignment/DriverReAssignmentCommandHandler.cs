using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Core.Extensions.Kernel;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverOrderRejection;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrderRejection;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverReAssignment;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverRequest;
using Delivery.Driver.Domain.Strategies.DriverAssignmentStrategy;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment
{
    public record DriverReAssignmentCommand(DriverReAssignmentCreationContract DriverReAssignmentCreationContract);
    
    public class DriverReAssignmentCommandHandler : ICommandHandler<DriverReAssignmentCommand, DriverReAssignmentCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverReAssignmentCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverReAssignmentCreationStatusContract> HandleAsync(DriverReAssignmentCommand command)
        {
            await using var databaseContext =
                await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var order = await databaseContext.Orders.FirstOrDefaultAsync(x =>
                x.ExternalId == command.DriverReAssignmentCreationContract.OrderId);

            var driverOrder = await databaseContext.DriverOrders
                .Where(x => x.OrderId == order.Id && x.Status == DriverOrderStatus.None)
                .OrderByDescending(x => x.InsertionDateTime)
                .Include(x => x.Driver)
                .FirstOrDefaultAsync();

            await RejectOrderAndRequestDriverAsync(driverOrder, order.ExternalId, databaseContext);

            var driverReAssignmentDispatcher =
                new DriverReAssignmentDispatcher(serviceProvider, executingRequestContextAdapter);

            var driverReAssignmentCreationStatusContract = driverOrder.Status switch
            {
                DriverOrderStatus.None => await driverReAssignmentDispatcher.DispatchAsync(driverOrder.Id, 3, DriverOrderStatus.None),
                DriverOrderStatus.Accepted => await driverReAssignmentDispatcher.DispatchAsync(driverOrder.Id, 60, DriverOrderStatus.Accepted),
                DriverOrderStatus.InProgress => await driverReAssignmentDispatcher.DispatchAsync(driverOrder.Id, 60, DriverOrderStatus.Accepted),
                DriverOrderStatus.Complete => new DriverReAssignmentCreationStatusContract {DriverId = driverOrder.Driver.ExternalId, OrderId = driverOrder.Order.ExternalId, AssignedDateTime = DateTimeOffset.UtcNow, DriverOrderStatus = DriverOrderStatus.Complete},
                DriverOrderStatus.Rejected => await driverReAssignmentDispatcher.DispatchAsync(driverOrder.Id, 3, DriverOrderStatus.Rejected),
                DriverOrderStatus.SystemRejected => await driverReAssignmentDispatcher.DispatchAsync(driverOrder.Id, 3, DriverOrderStatus.SystemRejected),
                _ => throw new InvalidOperationException(
                        $"{nameof(DriverOrderStatus)} must be valid. Instead: {driverOrder.Status}")
                    .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties())
            };
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(DriverReAssignmentCommandHandler)} executed. Output: {nameof(driverReAssignmentCreationStatusContract)}: {driverReAssignmentCreationStatusContract.ConvertToJson()}", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());

            if (!driverReAssignmentCreationStatusContract.DriverOrderStatus.Equals(DriverOrderStatus.Complete))
            {
                await new DriverReAssignmentMessagePublisher(serviceProvider)
                    .PublishAsync(new DriverReAssignmentMessage
                    {
                        PayloadIn = command.DriverReAssignmentCreationContract,
                        RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
                    });
            }

            return driverReAssignmentCreationStatusContract;
        }

        private async Task RejectOrderAndRequestDriverAsync(DriverOrder driverOrder, string orderExternalId, PlatformDbContext databaseContext)
        {
            var dateTimeDiff = driverOrder.InsertionDateTime.Subtract(DateTimeOffset.UtcNow);
            var totalDifferentMinute = dateTimeDiff.TotalMinutes;

            // if driver is not accepting after 3 minutes
            if (totalDifferentMinute > 3)
            {
                driverOrder.Status = DriverOrderStatus.SystemRejected;
                driverOrder.Reason = "System rejected";

                driverOrder.Driver.IsOrderAssigned = false;
                await databaseContext.SaveChangesAsync();
                
                var driverRequestMessageContract = new DriverRequestMessageContract
                {
                    PayloadIn = new DriverRequestContract {OrderId = orderExternalId},
                    PayloadOut = new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow},
                    RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
                };
                
                // request driver
                await new DriverRequestMessagePublisher(serviceProvider, executingRequestContextAdapter).ExecuteAsync(driverRequestMessageContract);
            }
        }
    }
}