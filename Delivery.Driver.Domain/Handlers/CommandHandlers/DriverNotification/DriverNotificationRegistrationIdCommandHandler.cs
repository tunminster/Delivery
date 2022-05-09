using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Notifications.Constants;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverNotification
{
    public record DriverNotificationRegistrationIdCommand(string Handle);
    
    public class DriverNotificationRegistrationIdCommandHandler : ICommandHandler<DriverNotificationRegistrationIdCommand, string>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverNotificationRegistrationIdCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<string> HandleAsync(DriverNotificationRegistrationIdCommand command)
        {
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationDriverHubName, NotificationHubConstants.NotificationDriverHubConnectionStringName);
            
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