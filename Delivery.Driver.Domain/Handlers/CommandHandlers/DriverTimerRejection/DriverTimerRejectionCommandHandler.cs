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
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverNotifications;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverElasticSearch;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverNotification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverTimerRejection
{
    public record DriverTimerRejectionCommand(string ShardKey);
    
    public class DriverTimerRejectionCommandHandler : ICommandHandler<DriverTimerRejectionCommand, StatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverTimerRejectionCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> Handle(DriverTimerRejectionCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var driverResponseThreshold = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSettingOrDefault<string>("DriverResponseThreshold", "2");
            
            var driverOrders =
                await databaseContext.DriverOrders.Where(x =>
                    x.Status == DriverOrderStatus.None && x.InsertionDateTime.AddMinutes(int.Parse(driverResponseThreshold)) > DateTimeOffset.UtcNow)
                    .Include(x => x.Driver)
                    .Include(x => x.Order)
                    .ToListAsync();

            foreach (var driverOrder in driverOrders)
            {
                driverOrder.Status = DriverOrderStatus.Rejected;
                driverOrder.Reason = "System rejected";

                driverOrder.Driver.IsOrderAssigned = false;
            }

            await databaseContext.SaveChangesAsync();

            foreach (var driverOrder in driverOrders)
            {
                // indexing driver
                await new DriverIndexCommandHandler(serviceProvider, executingRequestContextAdapter).Handle(
                    new DriverIndexCommand(driverOrder.Driver.ExternalId));
                
                // send push notification

                var driverOrderRejectedNotificationContract = new DriverOrderRejectedNotificationContract
                {
                    PushNotificationType = PushNotificationType.DeliveryRejected,
                    OrderId = driverOrder.ExternalId,
                    StoreId = string.Empty,
                    StoreName = string.Empty
                };

                await new DriverSendOrderRejectionCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(new DriverSendOrderRejectionCommand(driverOrderRejectedNotificationContract,
                        driverOrder.Driver.ExternalId));
            }

            return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}