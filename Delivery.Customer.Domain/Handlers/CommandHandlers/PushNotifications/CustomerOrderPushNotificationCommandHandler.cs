using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Customer.Domain.Contracts.V1.Enums;
using Delivery.Customer.Domain.Contracts.V1.RestContracts.PushNotification;
using Delivery.Customer.Domain.Converters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Domain.Helpers;
using Delivery.Notifications.Constants;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Customer.Domain.Handlers.CommandHandlers.PushNotifications
{
    public record CustomerOrderPushNotificationCommand(string OrderId, OrderNotificationFilter OrderNotificationFilter);
    public class CustomerOrderPushNotificationCommandHandler : ICommandHandler<CustomerOrderPushNotificationCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public CustomerOrderPushNotificationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task HandleAsync(CustomerOrderPushNotificationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var order = await databaseContext.Orders.Where(x => x.ExternalId == command.OrderId)
                .Include(x => x.Store)
                .Include(x => x.Address)
                .Include(x => x.Customer).SingleOrDefaultAsync() ?? 
                        throw new InvalidOperationException($"Expected order by order id: {command.OrderId}");

            var customerOrderNotificationContract = order.ConvertToCustomerOrderArrivedNotificationContract(command.OrderNotificationFilter);
            
            var customerDevice =
                databaseContext.NotificationDevices.FirstOrDefault(x => x.UserEmail == order.Customer.Username) ??
                throw new InvalidOperationException($"Customer - {order.Customer.Username} hasn't registered notification feature");

            var pushNotificationMessage = command.OrderNotificationFilter switch
            {
                OrderNotificationFilter.Accept => "Your food is preparing",
                OrderNotificationFilter.Arrived => "Your order is arrived",
                OrderNotificationFilter.Delivered => "Your order is delivered",
                OrderNotificationFilter.ReadyToDeliver => "Your order is ready to be delivered",
                _ => "Your order is accepted"
            };
            
            var notificationSendModel = new NotificationSendModel<CustomerOrderArrivedNotificationContract>
            {
                Pns = customerDevice.Platform,
                Message = pushNotificationMessage,
                Data = customerOrderNotificationContract,
                ToTag = customerDevice.Tag,
                Username = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
                ShardKey = executingRequestContextAdapter.GetShard().Key,
                RingKey = executingRequestContextAdapter.GetRing().ToString()
            };
            
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationHubName, NotificationHubConstants.NotificationHubConnectionStringName);
            
            var statusCode = await notificationClient.SendNotificationToUser(notificationSendModel);
            
            
            if (statusCode is HttpStatusCode.Accepted or HttpStatusCode.Created or HttpStatusCode.OK)
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace( $"Order arrived to customer push notification sent to {customerDevice.UserEmail}",
                    SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
            }
            else
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(new InvalidOperationException($"Order arrived to customer push notification failed - Customer {customerDevice.UserEmail} and Order id {command.OrderId}"),
                    executingRequestContextAdapter.GetTelemetryProperties());
            }
        }
    }
}