using System.Collections.Generic;

namespace Delivery.Library.Twilio.Contracts
{
    /// <summary>
    ///  Twilio email verification contract
    /// </summary>
    public record TwilioEmailVerificationContract
    {
        /// <summary>
        ///  Person name to display in the verification email
        /// </summary>
        public string Username { get; init; } = string.Empty;

        /// <summary>
        ///  Email address
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        ///  From name
        /// </summary>
        public string FromName { get; init; } = string.Empty;
        
        /// <summary>
        ///  domain specific verification name
        /// </summary>
        public string VerificationName { get; init; } = string.Empty;
        
        /// <summary>
        ///  domain specific email subject
        /// </summary>
        public string Subject { get; init; } = string.Empty;

        /// <summary>
        ///  Set User properties
        /// </summary>
        public IDictionary<string, object> UserProperties { get; internal set; } = new Dictionary<string, object>();
    }
}