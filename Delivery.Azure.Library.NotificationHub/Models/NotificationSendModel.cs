using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;

namespace Delivery.Azure.Library.NotificationHub.Models
{
    public record NotificationSendModel<T> : NotificationBaseModel
    where T: IDataContract
    {
        /// <summary>
        ///  Platform notification system
        /// <example>fcm, pns</example>
        /// </summary>
        public string Pns { get; init; } = string.Empty;

        /// <summary>
        ///  Short message for push notification
        /// </summary>
        public string Message { get; init; } = string.Empty;
        
        /// <summary>
        ///  Push notification title
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        ///  Push notification data
        /// </summary>
        public T Data { get; init; }

        /// <summary>
        ///  User tag to receive push notification
        /// </summary>
        public string ToTag { get; init; } = string.Empty;
        
    }
}