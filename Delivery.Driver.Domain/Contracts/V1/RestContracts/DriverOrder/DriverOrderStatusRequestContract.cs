using System;
using Delivery.Database.Enums;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder
{
    /// <summary>
    ///  Request status of driver's order
    /// </summary>
    public record DriverOrderStatusRequestContract
    {
        /// <summary>
        ///  From date that driver accepted the order to deliver
        /// </summary>
        public DateTimeOffset FromDate { get; init; }
        
        /// <summary>
        ///  Driver order status to filter the result
        /// </summary>
        public DriverOrderStatus DriverOrderStatus { get; init; }
    }
}