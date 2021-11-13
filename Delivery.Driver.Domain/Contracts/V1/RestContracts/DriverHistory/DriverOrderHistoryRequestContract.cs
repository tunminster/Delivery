using System;
using Delivery.Database.Enums;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverHistory
{
    /// <summary>
    ///  Driver order history request contract
    /// </summary>
    public record DriverOrderHistoryRequestContract
    {
        /// <summary>
        ///  Order date from
        /// </summary>
        /// <example>{{orderDateFrom}}</example>
        public DateTimeOffset OrderDateFrom { get; init; }
        
        /// <summary>
        ///  Driver order status
        /// </summary>
        /// <example>{{driverOrderStatus}}</example>
        public DriverOrderStatus DriverOrderStatus { get; init; }
    }
}