using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
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
                Username = GetTag(executingRequestContextAdapter.GetAuthenticatedUser().UserEmail!),
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

        private static string GetTag(string username)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            var name = username.Split('@');
            var tag = rgx.Replace(name[0], "");
            return tag;
        }
    }
}