using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverNotifications;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;
using Delivery.Notifications.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverNotification
{
    public record DriverSendOrderDeliveryRequestCommand
        (DriverOrderNotificationContract DriverOrderNotificationContract);
    
    public class DriverSendOrderDeliveryRequestCommandHandler : ICommandHandler<DriverSendOrderDeliveryRequestCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public DriverSendOrderDeliveryRequestCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> Handle(DriverSendOrderDeliveryRequestCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var email = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail;
            
            var driver = await databaseContext.Drivers.SingleOrDefaultAsync(x => x.EmailAddress == email) 
                         ?? throw new InvalidOperationException($"Expected a driver by id: {email} ");

            var driverDevice =
                databaseContext.NotificationDevices.FirstOrDefault(x => x.UserEmail == driver.EmailAddress) ??
                throw new InvalidOperationException($"Driver - {driver.EmailAddress} hasn't registered notification feature");

            var order = databaseContext.Orders.Single(x =>
                x.ExternalId == command.DriverOrderNotificationContract.OrderId);
            
            var driverOrder =
                databaseContext.DriverOrders.FirstOrDefault(x =>
                    x.OrderId == order.Id && x.Status == DriverOrderStatus.None);

            if (driverOrder != null)
            {
                if (driverOrder.DriverId != driver.Id)
                {
                    driverOrder.Status = DriverOrderStatus.Rejected;
                    driverOrder.Reason = $"{nameof(DriverSendOrderDeliveryRequestCommandHandler)} rejected";
                }
            }
            
            var driverOrderEntity = new DriverOrder
            {
                DriverId = driver.Id,
                OrderId = order.Id,
                InsertionDateTime = DateTimeOffset.UtcNow,
                InsertedBy = $"{nameof(DriverSendOrderDeliveryRequestCommandHandler)}"
            };
            
            await databaseContext.DriverOrders.AddAsync(driverOrderEntity);
            await databaseContext.SaveChangesAsync();

            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationDriverHubName, NotificationHubConstants.NotificationDriverHubConnectionStringName);
            

            var notificationSendModel = new NotificationSendModel<DriverOrderRequestContract>
            {
                Pns = driverDevice.Platform,
                Message = "Request food delivery",
                Data = new DriverOrderRequestContract(),
                ToTag = driverDevice.Tag,
                Username = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
                ShardKey = executingRequestContextAdapter.GetShard().Key,
                RingKey = executingRequestContextAdapter.GetRing().ToString()
            };
            
            var statusCode = await notificationClient.SendNotificationToUser(notificationSendModel);
            if (statusCode == HttpStatusCode.Accepted || statusCode == HttpStatusCode.Created || statusCode == HttpStatusCode.OK)
            {
                return new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow };
            }
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(new InvalidOperationException($"Driver order rejection push notification failed - Driver name {driver.EmailAddress} and Order id {command.DriverOrderNotificationContract.OrderId}"),
                executingRequestContextAdapter.GetTelemetryProperties());
            
            return new StatusContract { Status = false, DateCreated = DateTimeOffset.UtcNow };
        }
    }
}