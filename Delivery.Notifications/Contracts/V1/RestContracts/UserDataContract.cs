using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Notifications.Contracts.V1.RestContracts
{
    /// <summary>
    ///  User data contract
    /// </summary>
    public record UserDataContract : IDataContract
    {
        /// <summary>
        ///  Message
        /// </summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        ///  Push notification type
        /// </summary>
        public PushNotificationType PushNotificationType { get; init; }
    }
}