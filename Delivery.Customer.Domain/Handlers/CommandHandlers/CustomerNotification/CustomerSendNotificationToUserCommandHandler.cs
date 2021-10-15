using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Customer.Domain.Contracts.V1.RestContracts.PushNotification;
using Delivery.Domain.CommandHandlers;
using Delivery.Notifications.Constants;
using Delivery.Notifications.Contracts.V1.RestContracts;

namespace Delivery.Customer.Domain.Handlers.CommandHandlers.CustomerNotification
{
    public record CustomerSendNotificationToUserCommand(NotificationRequestContract NotificationRequestContract);
    
    public class CustomerSendNotificationToUserCommandHandler : ICommandHandler<CustomerSendNotificationToUserCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public CustomerSendNotificationToUserCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task Handle(CustomerSendNotificationToUserCommand command)
        {
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationHubName, NotificationHubConstants.NotificationHubConnectionStringName);
            
            var notificationRequestContract = command.NotificationRequestContract;

            var notificationSendModel = new NotificationSendModel<IDataContract>
            {
                Pns = notificationRequestContract.Pns,
                Message = notificationRequestContract.Message,
                Data = new CustomerOrderNotificationContract(),
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