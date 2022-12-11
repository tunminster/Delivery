using System;
using Delivery.Database.Enums;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverReAssignment
{
    /// <summary>
    ///  Driver Reassignment creation status contract
    /// </summary>
    public record DriverReAssignmentCreationStatusContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>
        ///  Driver id
        /// </summary>
        public string DriverId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Assigned date time
        /// </summary>
        public DateTimeOffset AssignedDateTime { get; init; }
        
        /// <summary>
        ///  Status indicates whether driver is already completed the delivery
        /// </summary>
        public DriverOrderStatus DriverOrderStatus { get; init; }
    }
}