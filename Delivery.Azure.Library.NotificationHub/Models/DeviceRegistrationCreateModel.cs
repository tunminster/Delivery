using Delivery.Azure.Library.NotificationHub.Contracts.V1;

namespace Delivery.Azure.Library.NotificationHub.Models
{
    public record DeviceRegistrationCreateModel
    {
        /// <summary>
        ///  Unique registration id from Notification hub.
        /// </summary>
        public string RegistrationId { get; init; } = string.Empty;

        /// <summary>
        ///  Authenticated user to be register along with the device registration.
        /// </summary>
        public string Username { get; init; } = string.Empty;
        
        public string CorrelationId { get; init; } = string.Empty;
        
        public string ShardKey { get; init; } = string.Empty;

        public string RingKey { get; init; } = string.Empty;
        
        /// <summary>
        ///  Device registration contract
        /// </summary>
        public DeviceRegistrationContract DeviceRegistration { get; init; } = new();
    }
}