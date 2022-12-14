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
                .Where(x => x.OrderId == order.Id)
                .OrderByDescending(x => x.InsertionDateTime)
                .Include(x => x.Driver)
                .FirstOrDefaultAsync();
            
            var driverReAssignmentDispatcher =
                new DriverReAssignmentDispatcher(serviceProvider, executingRequestContextAdapter);

            var driverReAssignmentCreationStatusContract = driverOrder.Status switch
            {
                DriverOrderStatus.None => await driverReAssignmentDispatcher.DispatchAsync(driverOrder.Id, 2, DriverOrderStatus.None),
                DriverOrderStatus.Accepted => await driverReAssignmentDispatcher.DispatchAsync(driverOrder.Id, 60, DriverOrderStatus.Accepted),
                DriverOrderStatus.InProgress => await driverReAssignmentDispatcher.DispatchAsync(driverOrder.Id, 60, DriverOrderStatus.Accepted),
                DriverOrderStatus.Complete => new DriverReAssignmentCreationStatusContract {DriverId = driverOrder.Driver.ExternalId, OrderId = driverOrder.Order.ExternalId, AssignedDateTime = DateTimeOffset.UtcNow, DriverOrderStatus = DriverOrderStatus.Complete},
                DriverOrderStatus.Rejected => await driverReAssignmentDispatcher.DispatchAsync(driverOrder.Id, 2, DriverOrderStatus.Rejected),
                DriverOrderStatus.SystemRejected => await driverReAssignmentDispatcher.DispatchAsync(driverOrder.Id, 2, DriverOrderStatus.SystemRejected),
                _ => throw new InvalidOperationException(
                        $"{nameof(DriverOrderStatus)} must be valid. Instead: {driverOrder.Status}")
                    .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties())
            };
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(DriverReAssignmentCommandHandler)} executed. Output: {nameof(driverReAssignmentCreationStatusContract)}: {driverReAssignmentCreationStatusContract.ConvertToJson()}", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());

            // if the order is not complete, keep sending the message to perform the above strategy
            if (!driverReAssignmentCreationStatusContract.DriverOrderStatus.Equals(DriverOrderStatus.Complete))
            {
                await new DriverReAssignmentScheduledMessagePublisher(serviceProvider)
                    .PublishAsync(new DriverReAssignmentMessage
                    {
                        PayloadIn = command.DriverReAssignmentCreationContract,
                        RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
                    });
            }

            return driverReAssignmentCreationStatusContract;
        }
    }
}