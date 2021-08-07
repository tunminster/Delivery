using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Reset password status contract
    /// </summary>
    public record DriverResetPasswordStatusContract
    {
        // <summary>
        ///  Email address
        /// </summary>
        public string Email { get; init; } = string.Empty;
        
        /// <summary>
        ///  Verification status
        /// </summary>
        public string Status { get; init; } = string.Empty;
        
        /// <summary>
        ///  Valid status
        /// </summary>
        public bool Valid { get; init; }
        
        /// <summary>
        ///  Verification requested date.
        /// </summary>
        public DateTimeOffset DateCreated { get; init; }
    }
}