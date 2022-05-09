using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Notifications.Constants;
using Microsoft.Graph;

namespace Delivery.Notifications.Handlers.CommandHandlers.DeleteRegisterDevice
{
    public record DeleteRegisterCommand(string RegistrationId);
    public class DeleteRegisterCommandHandler : ICommandHandler<DeleteRegisterCommand, string>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public DeleteRegisterCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<string> HandleAsync(DeleteRegisterCommand command)
        {
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationHubName, NotificationHubConstants.NotificationHubConnectionStringName);

            var registrationDeleteModel = new RegistrationDeleteModel
            {
                RegistrationId = command.RegistrationId,
                CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
                ShardKey = executingRequestContextAdapter.GetShard().Key,
                RingKey = executingRequestContextAdapter.GetRing().ToString()
            };
            
            await notificationClient.DeleteRegistration(registrationDeleteModel);
            return command.RegistrationId;
        }
    }
}