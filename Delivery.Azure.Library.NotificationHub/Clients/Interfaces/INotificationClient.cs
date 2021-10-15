using System.Net;
using System.Threading.Tasks;
using Delivery.Azure.Library.Connection.Managers.Interfaces;
using Delivery.Azure.Library.NotificationHub.Clients.Contracts;
using Delivery.Azure.Library.NotificationHub.Connections.Interfaces;
using Delivery.Azure.Library.NotificationHub.Models;
using Microsoft.Azure.NotificationHubs;

namespace Delivery.Azure.Library.NotificationHub.Clients.Interfaces
{
    public interface INotificationClient
    {
        /// <summary>
        ///     Encapsulates information about the notification hub connection
        /// </summary>
        /// <remarks>This is managed by a registered <see cref="INotificationHubSenderConnectionManager" /></remarks>
        IConnectionMetadata ConnectionMetadata { get; }
        
        /// <summary>
        ///     Sends the device handle to request registration id.
        /// </summary>
        /// <param name="registrationIdCreationModel">The handle to send</param>
        Task<string> CreateRegistrationIdAsync(RegistrationIdCreationModel registrationIdCreationModel);
        
        /// <summary>
        ///     Sends the device to register
        /// </summary>
        /// <param name="deviceRegistrationCreateModel">Device to register</param>
        Task<RegistrationDescription> RegisterDeviceAsync(DeviceRegistrationCreateModel deviceRegistrationCreateModel);

        /// <summary>
        ///  Delete a registration
        /// </summary>
        /// <param name="registrationDeleteModel"></param>
        /// <returns></returns>
        Task DeleteRegistration(RegistrationDeleteModel registrationDeleteModel);

        /// <summary>
        ///  Send notification model
        /// </summary>
        /// <param name="notificationSendModel"></param>
        /// <returns></returns>
        Task<HttpStatusCode> SendNotificationToUser<T>(NotificationSendModel<T> notificationSendModel) where T : IDataContract;
    }
}