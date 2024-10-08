using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.NotificationHub.Clients;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.Notifications.Constants;
using Delivery.Notifications.Contracts.V1.RestContracts;
using Delivery.Notifications.Helpers;
using Delivery.Notifications.Model;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

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
        
        public async Task<DeviceRegistrationResponseContract> HandleAsync(DriverNotificationRegisterDeviceCommand command)
        {
            var notificationClient = await NotificationClient.CreateAsync(serviceProvider, NotificationHubConstants.NotificationDriverHubName, NotificationHubConstants.NotificationDriverHubConnectionStringName);

            var deviceRegistrationCreateModel = new DeviceRegistrationCreateModel
            {
                RegistrationId = command.RegisterDeviceModel.RegistrationId,
                Username = NotificationTagHelper.GetTag(executingRequestContextAdapter.GetAuthenticatedUser().UserEmail!),
                Tag = $"{executingRequestContextAdapter.GetShard().Key}driver{NotificationTagHelper.GetTag(executingRequestContextAdapter.GetAuthenticatedUser().UserEmail!)}",
                DeviceRegistration = command.RegisterDeviceModel.DeviceRegistration,
                CorrelationId = executingRequestContextAdapter.GetCorrelationId(),
                ShardKey = executingRequestContextAdapter.GetShard().Key,
                RingKey = executingRequestContextAdapter.GetRing().ToString()
            };
            
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var notificationDevice = databaseContext.NotificationDevices.FirstOrDefault(x =>
                x.UserEmail == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail!);

            if (notificationDevice == null)
            {
                notificationDevice = new NotificationDevice
                {
                    RegistrationId = deviceRegistrationCreateModel.RegistrationId,
                    Platform = command.RegisterDeviceModel.DeviceRegistration.Platform,
                    UserEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                    Tag = deviceRegistrationCreateModel.Tag
                };

                await databaseContext.NotificationDevices.AddAsync(notificationDevice);
                await databaseContext.SaveChangesAsync();
            }
            else
            {
                // delete old registration here
                await DeleteRegistrationAsync(notificationClient, notificationDevice);
                
                notificationDevice.RegistrationId = deviceRegistrationCreateModel.RegistrationId;
                notificationDevice.Platform = command.RegisterDeviceModel.DeviceRegistration.Platform;
                notificationDevice.Tag = deviceRegistrationCreateModel.Tag;
                notificationDevice.UserEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail;
                await databaseContext.SaveChangesAsync();
            }
            
            var registrationDescription = await notificationClient.RegisterDeviceAsync(deviceRegistrationCreateModel);
            var deviceRegistrationResponseContract = new DeviceRegistrationResponseContract
            {
                Id = registrationDescription.RegistrationId
            };

            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace($"{nameof(NotificationClient)} set the device : {deviceRegistrationCreateModel.ConvertToJson()}", 
                        SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());

            return deviceRegistrationResponseContract;
        }
        
        private async Task DeleteRegistrationAsync(NotificationClient notificationClient,
            NotificationDevice? notificationDevice)
        {
            try
            {
                await notificationClient.DeleteRegistration(new RegistrationDeleteModel
                    { RegistrationId = notificationDevice!.RegistrationId });
            }
            catch (Exception ex)
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackException(ex, executingRequestContextAdapter.GetTelemetryProperties());
            }
        }
    }
}