using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverApproval
{
    /// <summary>
    ///  Driver approval contract
    /// </summary>
    public record DriverApprovalStatusContract
    {
        /// <summary>
        ///  Date created
        /// </summary>
        public DateTimeOffset DateCreated { get; init; }

        /// <summary>
        ///  Driver name
        /// </summary>
        public string DriverName { get; init; } = string.Empty;

        /// <summary>
        ///  Email address
        /// </summary>
        public string EmailAddress { get; init; } = string.Empty;
    }
}