using System;

namespace Delivery.Notifications.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Push notification request contract
    /// </summary>
    public record PushNotificationRequestContract
    {
        /// <summary>
        ///  Payload message for notification
        /// <example>{{payload}}</example>
        /// </summary>
        public string Payload { get; init; } = string.Empty;
        
        /// <summary>
        ///  Notification action
        /// <example>{{action}}</example>
        /// </summary>
        public string Action { get; init; } = string.Empty;
        
        /// <summary>
        ///  Tags
        /// <example>{{tags}}</example>
        /// </summary>
        public string[] Tags { get; init; } = Array.Empty<string>();
        
        /// <summary>
        ///  Silent
        /// <example>{{silent}}</example>
        /// </summary>
        public bool Silent { get; init; }
    }
}