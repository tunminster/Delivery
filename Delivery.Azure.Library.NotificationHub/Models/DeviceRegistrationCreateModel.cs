using Delivery.Azure.Library.NotificationHub.Contracts.V1;

namespace Delivery.Azure.Library.NotificationHub.Models
{
    public record DeviceRegistrationCreateModel : NotificationBaseModel
    {
        /// <summary>
        ///  Unique registration id from Notification hub.
        /// </summary>
        public string RegistrationId { get; init; } = string.Empty;

        /// <summary>
        ///  System generated unique tag
        /// </summary>
        public string Tag { get; init; } = string.Empty;
        
        /// <summary>
        ///  Device registration contract
        /// </summary>
        public DeviceRegistrationContract DeviceRegistration { get; init; } = new();
    }
}