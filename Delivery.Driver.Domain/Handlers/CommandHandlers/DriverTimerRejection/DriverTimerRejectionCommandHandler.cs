using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverOrderRejection;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverNotifications;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrderRejection;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverIndex;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverNotification;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverRequest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverTimerRejection
{
    public record DriverTimerRejectionCommand(string ShardKey);
    
    // replace with re-schedule message publishing
    // EG: 1. raise a message to find driver
    // 2. at the sametime to raise a (2 minutes later)schedule message to check order has been picked by the selected developer.
    // 3. If the drive is not picked up, try to find another driver. 
    public class DriverTimerRejectionCommandHandler : ICommandHandler<DriverTimerRejectionCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverTimerRejectionCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> HandleAsync(DriverTimerRejectionCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var driverResponseThreshold = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault<string>("DriverResponseThreshold", "5");
            
            var driverOrders =
                await databaseContext.DriverOrders.Where(x =>
                    x.Status == DriverOrderStatus.None && x.InsertionDateTime.AddMinutes(int.Parse(driverResponseThreshold)) < DateTimeOffset.UtcNow)
                    .Include(x => x.Driver)
                    .Include(x => x.Order)
                    .ToListAsync();
            
            
            if (driverOrders.Count == 0)
            {
                return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
            }

            foreach (var driverOrder in driverOrders)
            {
                driverOrder.Status = DriverOrderStatus.SystemRejected;
                driverOrder.Reason = "System rejected";

                driverOrder.Driver.IsOrderAssigned = false;
                await databaseContext.SaveChangesAsync();
                
                await new DriverIndexCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(new DriverIndexCommand(driverOrder.Driver.ExternalId));
                
                
                // var driverRequestMessageContract = new DriverRequestMessageContract
                // {
                //     PayloadIn = new DriverRequestContract {OrderId = driverOrder.Order.ExternalId},
                //     PayloadOut = statusContract,
                //     RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
                // };
                
                // request another driver  
                // reason of comment - orders are being requested after completed.
                //await new DriverRequestMessagePublisher(serviceProvider, executingRequestContextAdapter).ExecuteAsync(driverRequestMessageContract);
            }

            

            foreach (var driverOrder in driverOrders)
            {
                // indexing driver
                await new DriverIndexCommandHandler(serviceProvider, executingRequestContextAdapter).HandleAsync(
                    new DriverIndexCommand(driverOrder.Driver.ExternalId));
                
                // send push notification

                var driverOrderRejectedNotificationContract = new DriverOrderRejectedNotificationContract
                {
                    PushNotificationType = PushNotificationType.DeliveryRejected,
                    OrderId = driverOrder.ExternalId,
                    StoreId = string.Empty,
                    StoreName = string.Empty
                };
                
                // you don't need to send push notification for rejection
                // Send delivery rejection push notification to shop owner
                // await new DriverSendOrderRejectionCommandHandler(serviceProvider, executingRequestContextAdapter)
                //     .HandleAsync(new DriverSendOrderRejectionCommand(driverOrderRejectedNotificationContract,
                //         driverOrder.Driver.ExternalId));
            }

            return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}