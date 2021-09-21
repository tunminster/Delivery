using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;
using Delivery.Notifications.Constants;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopOrderManagement
{
    public record ShopOrderDriverRequestPushNotificationCommand(ShopOrderDriverRequestPushNotificationContract ShopOrderDriverRequestPushNotificationContract, int DriverId);
    
    public class ShopOrderDriverRequestPushNotificationCommandHandler : ICommandHandler<ShopOrderDriverRequestPushNotificationCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopOrderDriverRequestPushNotificationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> Handle(ShopOrderDriverRequestPushNotificationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var driver = await databaseContext.Drivers.SingleOrDefaultAsync(x => x.Id == command.DriverId) 
                         ?? throw new InvalidOperationException($"Expected a driver by id: {command.DriverId} ");

            var driverDevice =
                databaseContext.NotificationDevices.FirstOrDefault(x => x.UserEmail == driver.EmailAddress) ??
                throw new InvalidOperationException($"Driver - {driver.EmailAddress} hasn't registered notification feature");
            
            
            var notificationSendModel = new NotificationSendModel<IDataContract>
            {
                Pns = driverDevice.Platform,
                Message = "Request food delivery",
                Data = command.ShopOrderDriverRequestPushNotificationContract,
                ToTag = driverDevice.Tag,
                Username = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
                ShardKey = executingRequestContextAdapter.GetShard().Key,
                RingKey = executingRequestContextAdapter.GetRing().ToString()
            };
            
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationDriverHubName, NotificationHubConstants.NotificationDriverHubConnectionStringName);
            
            var statusCode = await notificationClient.SendNotificationToUser(notificationSendModel);
            if (statusCode == HttpStatusCode.Accepted || statusCode == HttpStatusCode.Created || statusCode == HttpStatusCode.OK)
            {
                return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
            }
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(new InvalidOperationException($"Driver order request push notification failed - Driver name {driver.EmailAddress} and Order id {command.ShopOrderDriverRequestPushNotificationContract.OrderId}"),
                 executingRequestContextAdapter.GetTelemetryProperties());
            
            return new StatusContract { Status = false, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}