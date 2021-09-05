using System.Collections.Generic;
using Delivery.Database.Enums;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders
{
    /// <summary>
    ///  Shop order contract which have information to fulfil the order
    /// </summary>
    public record ShopOrderContract
    {
        /// <summary>
        ///  Store id
        /// <example>{{storeId}}</example>
        /// </summary>
        public string StoreId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Order id
        /// <example>{{orderId}}</example>
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        /// Order type
        /// <example>{{orderType}}</example>
        /// </summary>
        public OrderType OrderType { get; init; }
        
        /// <summary>
        ///  Shop order items
        ///  <example>{{shopOrderItems}}</example>
        /// </summary>
        public List<ShopOrderItemContract> ShopOrderItems { get; init; } = new();
        
        /// <summary>
        ///  Total price
        ///  <example>{{totalPrice}}</example>
        /// </summary>
        public double TotalPrice { get; init; }
        
        /// <summary>
        ///  Shop order driver
        ///  <example>{{shopOrderDriver}}</example>
        /// </summary>
        public ShopOrderDriverContract? ShopOrderDriver { get; init; }
    }
}