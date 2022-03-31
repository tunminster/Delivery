using System;

namespace Delivery.Azure.Library.Communications.SendGrid.Contracts.V1.RestContracts
{
    public record BaseEmailCreationContract
    {
        /// <summary>
        ///  The unique id of this email 
        /// </summary>
        public string Id { get; init; } = string.Empty;
        
        /// <summary>
        ///  The date of this notification initiated
        /// </summary>
        public DateTimeOffset InitiateOn { get; init; }

    }
}