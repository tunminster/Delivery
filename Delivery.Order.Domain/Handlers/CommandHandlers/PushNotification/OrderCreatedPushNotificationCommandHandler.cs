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
using Delivery.Notifications.Constants;
using Delivery.Order.Domain.Contracts.RestContracts.PushNotification;
using Delivery.Order.Domain.Converters;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.PushNotification
{
    public record OrderCreatedPushNotificationCommand(
        OrderCreatedPushNotificationRequestContract OrderCreatedPushNotificationRequestContract);
    
    public class OrderCreatedPushNotificationCommandHandler : ICommandHandler<OrderCreatedPushNotificationCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public OrderCreatedPushNotificationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task Handle(OrderCreatedPushNotificationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var order = await databaseContext.Orders
                .Where(x =>
                    x.ExternalId == command.OrderCreatedPushNotificationRequestContract.OrderId)
                .Include(x => x.Store)
                .Include(x => x.Address)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .SingleOrDefaultAsync() ?? throw new
                InvalidOperationException(
                    $"Expected an order by orderId {command.OrderCreatedPushNotificationRequestContract.OrderId}");

            var orderCreatedPushNotificationContract = order.ConvertToContract();

            var shopOwnerUser = databaseContext.StoreUsers.FirstOrDefault(x => x.StoreId == order.Store.Id) ?? throw new InvalidOperationException($"Expected store user for the store id {order.Store.ExternalId}");
            
            var shopOwnerDevice =
                databaseContext.NotificationDevices.FirstOrDefault(x => x.UserEmail == shopOwnerUser.Username) ??
                throw new InvalidOperationException($"Shop owner - {shopOwnerUser.Username} hasn't registered notification feature");
            
            var notificationSendModel = new NotificationSendModel<OrderCreatedPushNotificationContract>
            {
                Pns = shopOwnerDevice.Platform,
                Message = "New order",
                Data = orderCreatedPushNotificationContract,
                ToTag = shopOwnerDevice.Tag,
                Username = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
                ShardKey = executingRequestContextAdapter.GetShard().Key,
                RingKey = executingRequestContextAdapter.GetRing().ToString()
            };
            
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationShopHubName, NotificationHubConstants.NotificationShopHubConnectionStringName);
            
            var statusCode = await notificationClient.SendNotificationToUser(notificationSendModel);
            
            if (statusCode is HttpStatusCode.Accepted or HttpStatusCode.Created or HttpStatusCode.OK)
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace( $"New order push notification sent to {shopOwnerUser.Username}",
                    SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
            }
            else
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(new InvalidOperationException($"A new order request push notification failed - Shop owner name {shopOwnerUser.Username} and Order id {command.OrderCreatedPushNotificationRequestContract.OrderId}"),
                    executingRequestContextAdapter.GetTelemetryProperties());
            }
            
            
        }
    }
}