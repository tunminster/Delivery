using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment
{
    /// <summary>
    ///  Driver assignment status contract
    /// </summary>
    public record DriverAssignmentStatusContract
    {
        /// <summary>
        ///  Date created
        /// #{CurrentFixedDateTimeUtc:date}#
        /// </summary>
        public DateTimeOffset DateCreated { get; init; }
    }
}