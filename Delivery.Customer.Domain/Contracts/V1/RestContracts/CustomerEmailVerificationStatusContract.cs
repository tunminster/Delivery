using System;

namespace Delivery.Customer.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Customer email verification status contract
    /// </summary>
    public record CustomerEmailVerificationStatusContract
    {
        /// <summary>
        ///  Verification status
        /// </summary>
        public string Status { get; init; } = string.Empty;

        /// <summary>
        ///  Verify email address
        /// </summary>
        public string UserEmailAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Valid status
        /// </summary>
        public bool? Valid { get; init; }
        
        /// <summary>
        ///  Verification requested date.
        /// </summary>
        public DateTimeOffset DateCreated { get; init; }
    }
}