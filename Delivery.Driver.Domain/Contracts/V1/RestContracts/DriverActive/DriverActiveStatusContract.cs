using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverActive
{
    /// <summary>
    ///  Driver active status contract
    /// </summary>
    public record DriverActiveStatusContract
    {
        /// <summary>
        ///  Is active
        /// </summary>
        public bool IsActive { get; init; }
        
        /// <summary>
        ///  Active created
        /// </summary>
        public DateTimeOffset DateCreated { get; init; }
    }
}