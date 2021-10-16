using System;

namespace Delivery.Managements.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Management user email verification status contract
    /// </summary>
    public record ManagementUserEmailVerificationStatusContract
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