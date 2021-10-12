using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Notifications.Constants;

namespace Delivery.Customer.Domain.Handlers.CommandHandlers.CustomerNotification
{
    public record CustomerNotificationRegistrationIdCommand(string Handle);
    
    public class CustomerNotificationRegistrationIdCommandHandler : ICommandHandler<CustomerNotificationRegistrationIdCommand, string>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public CustomerNotificationRegistrationIdCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<string> Handle(CustomerNotificationRegistrationIdCommand command)
        {
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationHubName, NotificationHubConstants.NotificationHubConnectionStringName);
            
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