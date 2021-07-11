using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Notifications.Constants;
using Delivery.Notifications.Contracts.V1.RestContracts;
using Delivery.Notifications.Model;

namespace Delivery.Notifications.Handlers.CommandHandlers.RegisterDevice
{
    public record RegisterDeviceCommand(RegisterDeviceModel RegisterDeviceModel);
    
    public class RegisterDeviceCommandHandler : ICommandHandler<RegisterDeviceCommand,DeviceRegistrationResponseContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public RegisterDeviceCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DeviceRegistrationResponseContract> Handle(RegisterDeviceCommand command)
        {
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationHubName, NotificationHubConstants.NotificationHubConnectionStringName);

            var deviceRegistrationCreateModel = new DeviceRegistrationCreateModel
            {
                RegistrationId = command.RegisterDeviceModel.RegistrationId,
                Username = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                DeviceRegistration = command.RegisterDeviceModel.DeviceRegistration,
                CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
                ShardKey = executingRequestContextAdapter.GetShard().Key,
                RingKey = executingRequestContextAdapter.GetRing().ToString()
            };
            
            var registrationDescription = await notificationClient.RegisterDeviceAsync(deviceRegistrationCreateModel);
            var deviceRegistrationResponseContract = new DeviceRegistrationResponseContract
            {
                Id = registrationDescription.RegistrationId
            };

            return deviceRegistrationResponseContract;
        }
    }
}