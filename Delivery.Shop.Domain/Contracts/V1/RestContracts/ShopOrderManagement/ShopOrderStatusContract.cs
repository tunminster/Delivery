using System;
using Delivery.Database.Enums;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement
{
    /// <summary>
    ///  Shop order status
    /// </summary>
    public record ShopOrderStatusContract
    {
        /// <summary>
        ///  Order id
        /// <example>{{orderId}}</example>
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Order status
        /// <example>{{orderStatus}}</example>
        /// </summary>
        public OrderStatus OrderStatus { get; init; }
        
        /// <summary>
        ///  Status
        /// <example>{{status}}</example>
        /// </summary>
        public bool Status { get; init; }
        
        /// <summary>
        ///  Date created
        /// <example>{{dateCreated}}</example>
        /// </summary>
        public DateTimeOffset DateCreated { get; init; }
    }
}