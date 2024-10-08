using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Notifications.Constants;
using Delivery.Notifications.Contracts.V1.RestContracts;

namespace Delivery.Notifications.Handlers.CommandHandlers.SendNotificationToUser
{
    public record SendNotificationToUserCommand(NotificationRequestContract NotificationRequestContract);
    
    public class SendNotificationToUserCommandHandler : ICommandHandler<SendNotificationToUserCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public SendNotificationToUserCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        
        public async Task HandleAsync(SendNotificationToUserCommand command)
        {
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationHubName, NotificationHubConstants.NotificationHubConnectionStringName);

            var notificationRequestContract = command.NotificationRequestContract;

            var notificationSendModel = new NotificationSendModel<UserDataContract>
            {
                Pns = notificationRequestContract.Pns,
                Message = notificationRequestContract.Message,
                Data = new UserDataContract(),
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