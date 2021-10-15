using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.NotificationHub.Clients.Contracts;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Connections;
using Delivery.Azure.Library.NotificationHub.Contracts;
using Delivery.Azure.Library.NotificationHub.Contracts.V1;
using Delivery.Azure.Library.NotificationHub.Models;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Enums;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Measurements.Dependencies.Models;
using Delivery.Azure.Library.Telemetry.Constants;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using Delivery.Azure.Library.NotificationHub.Contracts.V1;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Azure.Library.NotificationHub.Clients
{
    public class NotificationClient : NotificationHubSenderMiddleware, INotificationClient
    {
        private readonly IServiceProvider serviceProvider;
        protected NotificationClient(IServiceProvider serviceProvider, NotificationHubSenderConnection notificationHubSenderConnection) 
            : base(serviceProvider, notificationHubSenderConnection)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>
        ///     Creates a new instance of the <see cref="NotificationClient" />
        /// </summary>
        /// <param name="serviceProvider">The kernel</param>
        /// <param name="hubName">Name of the entity</param>
        /// <param name="connectionStringName">Name of the connection string to use</param>
        public static async Task<NotificationClient> CreateAsync(IServiceProvider serviceProvider, string hubName, string connectionStringName)
        {
            var connection = await GetConnectionTaskAsync(serviceProvider, hubName, connectionStringName);

            return new NotificationClient(serviceProvider, connection);
        }
        
        /// <summary>
        ///  This creates a registration id
        /// </summary>
        /// <param name="registrationIdCreationModel"></param>
        /// <returns></returns>
        public async Task<string> CreateRegistrationIdAsync(RegistrationIdCreationModel registrationIdCreationModel)
        {
            var dependencyName = NotificationHubSenderConnection.Hub.GetBaseUri();
            var dependencyData = new DependencyData("CreateNotificationRegistrationId", registrationIdCreationModel.Handle);
            var dependencyTarget = NotificationHubSenderConnection.Hub.GetBaseUri().AbsolutePath;

            var hub = NotificationHubSenderConnection.Hub;
            
            string newRegistrationId = null;
            
            var telemetryContextProperties = new Dictionary<string, string>
            {
                {"Body", registrationIdCreationModel.Handle},
                {CustomProperties.CorrelationId, registrationIdCreationModel.CorrelationId},
                {CustomProperties.Shard, registrationIdCreationModel.ShardKey},
                {CustomProperties.Ring, registrationIdCreationModel.RingKey}
            };
            
            // make sure there are no existing registrations for this push handle (used for iOS and Android)
            if (!string.IsNullOrEmpty(registrationIdCreationModel.Handle))
            {
                var registrations = await hub.GetRegistrationsByChannelAsync(registrationIdCreationModel.Handle, 100);
                foreach (RegistrationDescription registration in registrations)
                {
                    if (newRegistrationId == null)
                    {
                        newRegistrationId = registration.RegistrationId;
                    }
                    else
                    {
                        await hub.DeleteRegistrationAsync(registration);
                    }
                }
            }

            if (newRegistrationId == null)
            {
                newRegistrationId = await new DependencyMeasurement(serviceProvider)
                    .ForDependency(dependencyName.ToString(), MeasuredDependencyType.AzureNotificationHub,
                        dependencyData.ConvertToJson(), dependencyTarget)
                    .WithContextualInformation(telemetryContextProperties)
                    .TrackAsync(async () =>
                    {
                        var registrationId = await CircuitBreaker.CommunicateWithNotificationHubAsync(async () => 
                            await hub.CreateRegistrationIdAsync(), 
                            GetConnectionRenewalPolicy());

                        return registrationId;
                    });
            }
            
            return newRegistrationId;
        }

        // This creates or updates a registration (with provided channelURI) at the specified id
        public async Task<RegistrationDescription> RegisterDeviceAsync(DeviceRegistrationCreateModel deviceRegistrationCreateModel)
        {
            RegistrationDescription registration = null;
            
            var hub = NotificationHubSenderConnection.Hub;
            
            var dependencyName = NotificationHubSenderConnection.Hub.GetBaseUri();
            var dependencyData = new DependencyData("RegisterNotificationDevice", deviceRegistrationCreateModel.ConvertToJson());
            var dependencyTarget = NotificationHubSenderConnection.Hub.GetBaseUri().AbsolutePath;
            var telemetryContextProperties = new Dictionary<string, string>
            {
                {"Body", deviceRegistrationCreateModel.DeviceRegistration.ConvertToJson()},
                {CustomProperties.CorrelationId, deviceRegistrationCreateModel.CorrelationId},
                {CustomProperties.Shard, deviceRegistrationCreateModel.ShardKey},
                {CustomProperties.Ring, deviceRegistrationCreateModel.RingKey}
            };
            
            var deviceUpdate = deviceRegistrationCreateModel.DeviceRegistration with
            {
                Tags = new []{ deviceRegistrationCreateModel.Tag}
            };
            switch (deviceUpdate.Platform)
            {
                case "mpns":
                    registration = new MpnsRegistrationDescription(deviceUpdate.Handle);
                    break;
                case "wns":
                    registration = new WindowsRegistrationDescription(deviceUpdate.Handle);
                    break;
                case "apns":
                    registration = new AppleRegistrationDescription(deviceUpdate.Handle);
                    break;
                case "fcm":
                    registration = new FcmRegistrationDescription(deviceUpdate.Handle);
                    break;
                default:
                    throw new NotImplementedException(HttpStatusCode.BadRequest.ToString());
            }

            registration.RegistrationId = deviceRegistrationCreateModel.RegistrationId;

            registration.Tags = new HashSet<string>(deviceUpdate.Tags);
            //registration.Tags.Add($"{deviceRegistrationCreateModel.Username}");

            try
            {
                registration = await new DependencyMeasurement(serviceProvider)
                    .ForDependency(dependencyName.ToString(), MeasuredDependencyType.AzureNotificationHub,
                        dependencyData.ConvertToJson(), dependencyTarget)
                    .WithContextualInformation(telemetryContextProperties)
                    .TrackAsync(async () => await hub.CreateOrUpdateRegistrationAsync(registration));
                
            }
            catch (MessagingException e)
            {
                ReturnGoneIfHubResponseIsGone(e);
            }
            
            return registration;
        }

        public async Task DeleteRegistration(RegistrationDeleteModel registrationDeleteModel)
        {
            var hub = NotificationHubSenderConnection.Hub;
            
            var dependencyName = NotificationHubSenderConnection.Hub.GetBaseUri();
            var dependencyData = new DependencyData("DeleteNotificationRegistration", registrationDeleteModel.ConvertToJson());
            var dependencyTarget = NotificationHubSenderConnection.Hub.GetBaseUri().AbsolutePath;
            var telemetryContextProperties = new Dictionary<string, string>
            {
                {"Body", registrationDeleteModel.ConvertToJson()},
                {CustomProperties.CorrelationId, registrationDeleteModel.CorrelationId},
                {CustomProperties.Shard, registrationDeleteModel.ShardKey},
                {CustomProperties.Ring, registrationDeleteModel.RingKey}
            };
            
            await new DependencyMeasurement(serviceProvider)
                .ForDependency(dependencyName.ToString(), MeasuredDependencyType.AzureNotificationHub,
                    dependencyData.ConvertToJson(), dependencyTarget)
                .WithContextualInformation(telemetryContextProperties)
                .TrackAsync(async () => await hub.DeleteRegistrationAsync(registrationDeleteModel.RegistrationId));
        }

        public async Task<HttpStatusCode> SendNotificationToUser<T>(NotificationSendModel<T> notificationSendModel)
        where T : IDataContract
        {
            var hub = NotificationHubSenderConnection.Hub;
            
            var dependencyName = NotificationHubSenderConnection.Hub.GetBaseUri();
            var dependencyData = new DependencyData("SendNotification", notificationSendModel.ConvertToJson());
            var dependencyTarget = NotificationHubSenderConnection.Hub.GetBaseUri().AbsolutePath;
            var telemetryContextProperties = new Dictionary<string, string>
            {
                {"Body", notificationSendModel.ConvertToJson()},
                {CustomProperties.CorrelationId, notificationSendModel.CorrelationId},
                {CustomProperties.Shard, notificationSendModel.ShardKey},
                {CustomProperties.Ring, notificationSendModel.RingKey}
            };
            
            var user = notificationSendModel.Username;
            string[] userTag = new string[1];
            userTag[0] = notificationSendModel.ToTag.ToLower();
            
            NotificationOutcome outcome = null;
            HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError;

            switch (notificationSendModel.Pns.ToLower())
            {
                case "wns":
                    // Windows 8.1 / Windows Phone 8.1
                    var toast = @"<toast><visual><binding template=""ToastText01""><text id=""1"">" + 
                                 notificationSendModel.Message + "</text></binding></visual></toast>";
                    
                    outcome = await new DependencyMeasurement(serviceProvider)
                        .ForDependency(dependencyName.ToString(), MeasuredDependencyType.AzureNotificationHub,
                            dependencyData.ConvertToJson(), dependencyTarget)
                        .WithContextualInformation(telemetryContextProperties)
                        .TrackAsync(async () => await hub.SendWindowsNativeNotificationAsync(toast, userTag));
                    break;
                case "apns":
                    // iOS
                    //var alert = "{\"aps\":{\"alert\":\"" + "From " + user + ": " + notificationSendModel.Message + "\"}}";
                    var apsMessageContract = new ApsMessageContract<IDataContract>
                        { Aps = new Aps{ Alert = notificationSendModel.Title}, Message = new ApsNotificationMessage {Message = notificationSendModel.Message},Data = notificationSendModel.Data };
                    outcome = await new DependencyMeasurement(serviceProvider)
                        .ForDependency(dependencyName.ToString(), MeasuredDependencyType.AzureNotificationHub,
                            dependencyData.ConvertToJson(), dependencyTarget)
                        .WithContextualInformation(telemetryContextProperties)
                        .TrackAsync(async () => await hub.SendAppleNativeNotificationAsync(apsMessageContract.ConvertToJson(), userTag));
                    break;
                case "fcm":
                    // Android
                    var gcmMessageContract = new GcmMessageContract<IDataContract>{ Notification = new NotificationTitle { Title = notificationSendModel.Title, Body =notificationSendModel.Message},
                        Data = notificationSendModel.Data};
                    
                    outcome = await new DependencyMeasurement(serviceProvider)
                        .ForDependency(dependencyName.ToString(), MeasuredDependencyType.AzureNotificationHub,
                            dependencyData.ConvertToJson(), dependencyTarget)
                        .WithContextualInformation(telemetryContextProperties)
                        .TrackAsync(async () => await hub.SendFcmNativeNotificationAsync(gcmMessageContract.ConvertToJson(), userTag));
                    break;
                case "gcm":
                    var gcmMessage= new GcmMessageContract<IDataContract>{ Notification = new NotificationTitle { Title = notificationSendModel.Title, Body =notificationSendModel.Message},
                    Data = notificationSendModel.Data};
                    await hub.SendFcmNativeNotificationAsync(gcmMessage.ConvertToJson(), userTag);
                    break;
            }
            
            if (outcome != null)
            {
                if (!((outcome.State == Microsoft.Azure.NotificationHubs.NotificationOutcomeState.Abandoned) ||
                      (outcome.State == Microsoft.Azure.NotificationHubs.NotificationOutcomeState.Unknown)))
                {
                    httpStatusCode = HttpStatusCode.OK;
                }
            }
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace( $"{nameof(NotificationClient)} sent push notification payload {notificationSendModel.ConvertToJson()} to user {notificationSendModel.ToTag.ToLower()}",
                SeverityLevel.Information, telemetryContextProperties);

            return httpStatusCode;
        }

        private static void ReturnGoneIfHubResponseIsGone(MessagingException e)
        {
            var webex = e.InnerException as WebException;
            if (webex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = (HttpWebResponse)webex.Response;
                if (response.StatusCode == HttpStatusCode.Gone)
                    throw new HttpRequestException(HttpStatusCode.Gone.ToString());
            }

            throw e;
        }
    }
}