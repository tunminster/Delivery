using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment
{
    /// <summary>
    ///  Driver assignment creation controct
    /// </summary>
    public record DriverAssignmentCreationContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public int OrderId { get; init; }
        
        /// <summary>
        ///  Driver id
        /// </summary>
        /// <example>{{driverId}}</example>
        public int DriverId { get; init; }
        
        /// <summary>
        ///  Created date
        ///	 #{CurrentFixedDateTimeUtc:date}#
        /// </summary>
        public DateTimeOffset DateCreated { get; init; }
        
    }
}