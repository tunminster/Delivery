using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverEarnings
{
    /// <summary>
    ///  Driver earning details query contract
    /// </summary>
    public record DriverEarningDetailsQueryContract
    {
        /// <summary>
        ///  Start date
        /// </summary>
        /// <example>{{startDate}}</example>
        public DateTimeOffset StartDate { get; init; }
        
        /// <summary>
        ///  End date
        /// </summary>
        /// <example>{{endDate}}</example>
        public DateTimeOffset EndDate { get; init; }
    }
}