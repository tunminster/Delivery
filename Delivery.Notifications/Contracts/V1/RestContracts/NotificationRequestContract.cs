namespace Delivery.Notifications.Contracts.V1.RestContracts
{
    public record NotificationRequestContract
    {
        /// <summary>
        ///  Platform notification service parameter
        /// </summary>
        public string Pns { get; init; } = string.Empty;

        /// <summary>
        ///  Notification message
        /// </summary>
        public string Message { get; init; } = string.Empty;
        
        /// <summary>
        ///  To set username
        /// </summary>
        public string ToTag { get; init; } = string.Empty;
    }
}