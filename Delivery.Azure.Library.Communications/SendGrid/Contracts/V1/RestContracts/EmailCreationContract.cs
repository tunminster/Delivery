using System;

namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    public record EmailCreationContract
    {
        /// <summary>
        ///  The unique id of this email 
        /// </summary>
        public string? NotificationUniqueId { get; init; } = string.Empty;
        
        /// <summary>
        ///  The date of this notification initiated
        /// </summary>
        public DateTimeOffset? InitiatedOn { get; init; }
        
        /// <summary>
        ///  The date of notification should be send at / scheduled for sending
        /// </summary>
        public DateTimeOffset? NotificationSendAt { get; init; }
        
        /// <summary>
        ///  The date of notification was sent at
        /// </summary>
        public DateTimeOffset? NotificationSentAt { get; init; }

        /// <summary>
        ///  Email address
        /// </summary>
        public string EmailAddress { get; init; } = string.Empty;

        /// <summary>
        ///  The version of this contract
        /// </summary>
        public virtual int Version => 1;

    }
}