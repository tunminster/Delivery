using System;
using System.Collections.Generic;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverHistory
{
    /// <summary>
    ///  Driver order history details contract
    /// </summary>
    public record DriverOrderHistoryDetailsContract
    {
        /// <summary>
        ///  Store name
        /// </summary>
        /// <example>{{storeName}}</example>
        public string StoreName { get; init; } = string.Empty;

        /// <summary>
        ///  Store address
        /// </summary>
        /// <example>{{storeAddress}}</example>
        public string StoreAddress { get; init; } = string.Empty;
        
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

        /// <summary>
        ///  Order items
        /// </summary>
        /// <example>{{orderItems}}</example>
        public List<DriverOrderHistoryItemContract> OrderItems { get; init; } = new();
    }
}