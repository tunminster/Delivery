using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverHistory
{
    /// <summary>
    ///  Order history contract
    /// </summary>
    public record DriverOrderHistoryContract
    {
        /// <summary>
        ///  Store name
        /// </summary>
        /// <example>{{storeName}}</example>
        public string StoreName { get; init; } = string.Empty;
        
        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        /// Order date
        /// </summary>
        /// <example>{{orderDate}}</example>
        public DateTimeOffset OrderDate { get; init; }
        
        /// <summary>
        ///  Delivery fee
        /// </summary>
        /// <example>{{deliveryFee}}</example>
        public int DeliveryFee { get; init; }
    }
}