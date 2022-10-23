using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverOrderRejection;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrderRejection;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverRequest;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverTimerAssignment
{
    public record DriverTimerAssignmentCommand(string ShardKey);

    public class DriverTimerAssignmentCommandHandler : CommandHandler<DriverTimerAssignmentCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public DriverTimerAssignmentCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        protected override async Task<StatusContract> HandleAsync(DriverTimerAssignmentCommand command)
        {
            await using var databaseContext =
                await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var orders =  await databaseContext.Orders.Where(x => x.OrderType == OrderType.DeliverTo
                                                                 && x.Status == OrderStatus.Ready
                                                                 ).ToListAsync();
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(DriverTimerAssignmentCommandHandler)} retrieves orders to be assigned.", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());

            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };

            foreach (var order in orders)
            {
                var driverOrder = await databaseContext.DriverOrders
                    .FirstOrDefaultAsync(x => x.OrderId == order.Id
                                              && x.Status != DriverOrderStatus.Rejected || x.Status != DriverOrderStatus.SystemRejected);
                
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace($"{nameof(DriverTimerAssignmentCommandHandler)}: driver is not empty.  Driver {driverOrder.ConvertToJson()}", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
                
                if (driverOrder == null)
                {
                    serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"{nameof(DriverTimerAssignmentCommandHandler)}'s driver order is empty. Driver Request message is raising.");
                    var driverRequestMessageContract = new DriverRequestMessageContract
                    {
                        PayloadIn = new DriverRequestContract {OrderId = order.ExternalId},
                        PayloadOut = statusContract,
                        RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
                    };
                
                    // request driver
                    await new DriverRequestMessagePublisher(serviceProvider, executingRequestContextAdapter).ExecuteAsync(driverRequestMessageContract);
                }
            }

            return statusContract;
        }
    }
}