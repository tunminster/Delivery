using Delivery.Azure.Library.NotificationHub.Contracts.V1;

namespace Delivery.Notifications.Model
{
    public record RegisterDeviceModel
    {
        public string RegistrationId { get; init; } = string.Empty;
        
        public DeviceRegistrationContract DeviceRegistration { get; init; }

        public string Username { get; init; } = string.Empty;

    }
}