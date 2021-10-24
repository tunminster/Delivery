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
        ///  Date created from
        /// </summary>
        /// <example>{{dateCreatedFrom}}</example>
        public DateTimeOffset DateCreatedFrom { get; init; }
        
        /// <summary>
        ///  Driver earning filter
        /// </summary>
        /// <example>{{driverEarningFilter}}</example>
        public DriverEarningFilter DriverEarningFilter { get; init; }
    }
}