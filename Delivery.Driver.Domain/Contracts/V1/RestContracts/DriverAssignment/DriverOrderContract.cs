using System;
using Delivery.Database.Enums;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment
{
    /// <summary>
    ///  Driver order contract
    /// </summary>
    public record DriverOrderContract
    {
        /// <summary>
        ///  Driver order id
        /// </summary>
        /// <example>{{id}}</example>
        public string Id { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver id
        /// </summary>
        /// <example>{{driverId}}</example>
        public string DriverId { get; init; } = string.Empty;
        
        /// <summary>
        /// Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Total amount
        /// </summary>
        /// <example>{{deliveryFee}}</example>
        public int DeliveryFee { get; init; }
        
        /// <summary>
        ///  Date created
        /// </summary>
        /// <example>{{dateCreated}}</example>
        public DateTimeOffset DateCreated { get; init; }
        
        /// <summary>
        ///  Status
        /// </summary>
        /// <example>{{status}}</example>
        public DriverOrderStatus Status { get; init; }
        
    }
}