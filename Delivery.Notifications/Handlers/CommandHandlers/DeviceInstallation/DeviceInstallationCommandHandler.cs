using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Notifications.Contracts.V1.RestContracts;
using Microsoft.Azure.NotificationHubs;

namespace Delivery.Notifications.Handlers.CommandHandlers.DeviceInstallation
{
    public record DeviceInstallationCommand(DeviceInstallationContract DeviceInstallationContract);
    
    public class DeviceInstallationCommandHandler : ICommandHandler<DeviceInstallationCommand,StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        private readonly Dictionary<string, NotificationPlatform> installtionPlatform;

        public DeviceInstallationCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
            installtionPlatform = new Dictionary<string, NotificationPlatform>
            {
                { nameof(NotificationPlatform.Apns).ToLower(), NotificationPlatform.Apns },
                { nameof(NotificationPlatform.Fcm).ToLower(), NotificationPlatform.Fcm }
            };
        }
        
        public Task<StatusContract> Handle(DeviceInstallationCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}