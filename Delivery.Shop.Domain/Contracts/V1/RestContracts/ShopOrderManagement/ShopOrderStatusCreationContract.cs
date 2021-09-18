using System;
using Delivery.Database.Enums;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement
{
    /// <summary>
    ///  Shop order creation contract to accept or reject order
    /// </summary>
    public record ShopOrderStatusCreationContract
    {
        /// <summary>
        ///  OrderId 
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Preparation time
        /// </summary>
        /// <example>{{preparationTime}}</example>
        public int PreparationTime { get; init; }
        
        /// <summary>
        ///  Pickup time
        ///  <example>{{pickupTime}}</example>
        /// </summary>
        public DateTimeOffset? PickupTime { get; init; }
        
        /// <summary>
        ///  Order status
        /// </summary>
        /// <example>{{orderStatus}}</example>
        public OrderStatus OrderStatus { get; init; }
    }
}