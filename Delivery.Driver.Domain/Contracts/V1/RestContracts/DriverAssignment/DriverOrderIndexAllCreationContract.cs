using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment
{
    /// <summary>
    ///  Driver order index all creation contract
    /// </summary>
    public record DriverOrderIndexAllCreationContract
    {
        /// <summary>
        ///  Create date
        /// </summary>
        /// <example>{{createDate}}</example>
        public DateTimeOffset CreateDate { get; init; }
    }
}