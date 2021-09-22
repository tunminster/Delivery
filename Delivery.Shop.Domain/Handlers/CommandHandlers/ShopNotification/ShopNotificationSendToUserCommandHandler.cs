using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Notifications.Constants;
using Delivery.Notifications.Contracts.V1.RestContracts;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;

namespace Delivery.Shop.Domain.Handlers.CommandHandlers.ShopNotification
{
    public record ShopNotificationSendToUserCommand(NotificationRequestContract NotificationRequestContract);
    public class ShopNotificationSendToUserCommandHandler : ICommandHandler<ShopNotificationSendToUserCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ShopNotificationSendToUserCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task Handle(ShopNotificationSendToUserCommand command)
        {
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationShopHubName, NotificationHubConstants.NotificationShopHubConnectionStringName);
            
            var notificationRequestContract = command.NotificationRequestContract;
            
            var notificationSendModel = new NotificationSendModel<IDataContract>
            {
                Pns = notificationRequestContract.Pns,
                Message = notificationRequestContract.Message,
                Data = new ShopOrderNotificationContract(),
                ToTag = notificationRequestContract.ToTag,
                Username = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
                ShardKey = executingRequestContextAdapter.GetShard().Key,
                RingKey = executingRequestContextAdapter.GetRing().ToString()
            };
            
            await notificationClient.SendNotificationToUser(notificationSendModel);
        }
    }
}