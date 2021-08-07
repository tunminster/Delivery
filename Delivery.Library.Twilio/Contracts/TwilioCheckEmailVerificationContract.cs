using System.Collections.Generic;

namespace Delivery.Library.Twilio.Contracts
{
    /// <summary>
    ///  To check verification code
    /// </summary>
    public record TwilioCheckEmailVerificationContract
    {
        /// <summary>
        ///  Email address
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        ///  Verification code
        /// </summary>
        public string Code { get; init; } = string.Empty;
        
        /// <summary>
        ///  Set User properties
        /// </summary>
        public IDictionary<string, object> UserProperties { get; internal set; } = new Dictionary<string, object>();
    }
}