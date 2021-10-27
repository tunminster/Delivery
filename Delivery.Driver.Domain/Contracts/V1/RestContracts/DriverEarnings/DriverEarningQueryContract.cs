using System;
using Delivery.Driver.Domain.Contracts.V1.Enums.DriverEarnings;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverEarnings
{
    /// <summary>
    ///  Driver earning query contract
    /// </summary>
    public record DriverEarningQueryContract
    {
        /// <summary>
        ///  Year 
        /// </summary>
        /// <example>{{year}}</example>
        public int Year { get; init; }
        
        /// <summary>
        ///  Month 
        /// </summary>
        /// <example>{{month}}</example>
        public int Month { get; init; }
        
        
        /// <summary>
        ///  Driver earning filter
        /// </summary>
        /// <example>{{driverEarningFilter}}</example>
        public DriverEarningFilter DriverEarningFilter { get; init; }
    }
}