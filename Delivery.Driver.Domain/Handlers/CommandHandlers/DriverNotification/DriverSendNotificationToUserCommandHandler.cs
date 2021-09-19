using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Notifications.Constants;
using Delivery.Notifications.Contracts.V1.RestContracts;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverNotification
{
    public record DriverSendNotificationToUserCommand(NotificationRequestContract NotificationRequestContract);
    
    public class DriverSendNotificationToUserCommandHandler : ICommandHandler<DriverSendNotificationToUserCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public DriverSendNotificationToUserCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task Handle(DriverSendNotificationToUserCommand command)
        {
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationDriverHubName, NotificationHubConstants.NotificationDriverHubConnectionStringName);
            
            var notificationRequestContract = command.NotificationRequestContract;

            var notificationSendModel = new NotificationSendModel
            {
                Pns = notificationRequestContract.Pns,
                Message = notificationRequestContract.Message,
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