using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Notifications.Constants;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopNotification
{
    public record ShopNotificationRegistrationIdCommand(string Handle);
    public class ShopNotificationRegistrationIdCommandHandler : ICommandHandler<ShopNotificationRegistrationIdCommand, string>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopNotificationRegistrationIdCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<string> Handle(ShopNotificationRegistrationIdCommand command)
        {
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationShopHubName, NotificationHubConstants.NotificationShopHubConnectionStringName);
            
            var registrationIdCreationModel = new RegistrationIdCreationModel
            {
                Handle = command.Handle,
                CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
                ShardKey = executingRequestContextAdapter.GetShard().Key,
                RingKey = executingRequestContextAdapter.GetRing().ToString()
            };

            var registrationId = await notificationClient.CreateRegistrationIdAsync(registrationIdCreationModel);
            return registrationId;
        }
    }
}