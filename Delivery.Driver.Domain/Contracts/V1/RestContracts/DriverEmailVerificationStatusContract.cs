using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Driver email verification status
    /// </summary>
    public record DriverEmailVerificationStatusContract
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