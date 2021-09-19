using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.Notifications.Constants;
using Delivery.Notifications.Contracts.V1.RestContracts;
using Delivery.Notifications.Helpers;
using Delivery.Notifications.Model;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverNotification
{
    public record DriverNotificationRegisterDeviceCommand(RegisterDeviceModel RegisterDeviceModel);
    public class DriverNotificationRegisterDeviceCommandHandler : ICommandHandler<DriverNotificationRegisterDeviceCommand,DeviceRegistrationResponseContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public DriverNotificationRegisterDeviceCommandHandler(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DeviceRegistrationResponseContract> Handle(DriverNotificationRegisterDeviceCommand command)
        {
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationDriverHubName, NotificationHubConstants.NotificationDriverHubConnectionStringName);

            var deviceRegistrationCreateModel = new DeviceRegistrationCreateModel
            {
                RegistrationId = command.RegisterDeviceModel.RegistrationId,
                Username = NotificationTagHelper.GetTag(executingRequestContextAdapter.GetAuthenticatedUser().UserEmail!),
                DeviceRegistration = command.RegisterDeviceModel.DeviceRegistration,
                CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
                ShardKey = executingRequestContextAdapter.GetShard().Key,
                RingKey = executingRequestContextAdapter.GetRing().ToString()
            };
            
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var notificationDevice = databaseContext.NotificationDevices.FirstOrDefault(x =>
                x.RegistrationId == deviceRegistrationCreateModel.RegistrationId &&
                x.Tag == deviceRegistrationCreateModel.Username);

            if (notificationDevice == null)
            {
                notificationDevice = new NotificationDevice
                {
                    RegistrationId = deviceRegistrationCreateModel.RegistrationId,
                    Platform = command.RegisterDeviceModel.DeviceRegistration.Platform,
                    Tag = deviceRegistrationCreateModel.Username
                };

                await databaseContext.NotificationDevices.AddAsync(notificationDevice);
                await databaseContext.SaveChangesAsync();
            }
            
            var registrationDescription = await notificationClient.RegisterDeviceAsync(deviceRegistrationCreateModel);
            var deviceRegistrationResponseContract = new DeviceRegistrationResponseContract
            {
                Id = registrationDescription.RegistrationId
            };

            return deviceRegistrationResponseContract;
        }
    }
}