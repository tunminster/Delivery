using Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.Enums;

namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Notification status contract
    /// </summary>
    public record NotificationStatusContract
    {
        /// <summary>
        ///  The unique id of this notification
        /// </summary>
        public string? NotificationUniqueId { get; init; }

        /// <summary>
        ///  Notification status
        /// </summary>
        public NotificationStatus Status { get; init; } = new();

    }
}