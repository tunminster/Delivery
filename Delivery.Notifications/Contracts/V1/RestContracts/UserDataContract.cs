using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;

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
    }
}