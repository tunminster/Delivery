namespace Delivery.Azure.Library.NotificationHub.Contracts.V1
{
    public record DeviceRegistrationContract
    {
        /// <summary>
        ///  Platform
        /// <example>wns or apns or fcm</example>
        /// </summary>
        public string Platform { get; init; } = string.Empty;
        
        /// <summary>
        /// Platform notification service PNS parameter
        /// </summary>
        public string Handle { get; init; } = string.Empty;

        public string[] Tags { get; init; }
        
    }
}