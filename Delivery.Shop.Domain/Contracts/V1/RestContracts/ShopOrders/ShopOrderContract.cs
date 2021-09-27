using System;
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
        /// Order status
        /// <example>{{status}}</example>
        /// </summary>
        public OrderStatus Status { get; init; }
        
        /// <summary>
        ///  Shop order items
        ///  <example>{{shopOrderItems}}</example>
        /// </summary>
        public List<ShopOrderItemContract> ShopOrderItems { get; init; } = new();
        
        /// <summary>
        ///  Subtotal without any additional charges
        /// <example>{{subtotal}}</example>
        /// </summary>
        public int Subtotal { get; init; }
        
        /// <summary>
        ///  Total price
        ///  It's two decimal
        ///  <example>{{totalPrice}}</example>
        /// </summary>
        public int TotalAmount { get; init; }
        
        /// <summary>
        ///  Platform fees to customer
        ///  <example>{{platformServiceFee}}</example>
        /// </summary>
        public int PlatformServiceFee { get; init; }
        
        /// <summary>
        ///  Delivery Fees
        ///  <example>{{deliveryFee}}</example>
        /// </summary>
        public int DeliveryFee { get; init; }
        
        /// <summary>
        ///  Tax fees
        /// <example>{{tax}}</example>
        /// </summary>
        public int Tax { get; init; }
        
        /// <summary>
        ///  Business service fee
        /// <example>{{businessServiceFee</example>
        /// </summary>
        public int BusinessServiceFee { get; init; }
        
        /// <summary>
        ///  Preparation time
        /// <example>{{preparationTime}}</example>
        /// </summary>
        public int PreparationTime { get; init; }
        
        /// <summary>
        ///  Pickup time
        /// <example>{{pickupTime}}</example>
        /// </summary>
        public DateTimeOffset PickupTime { get; init; }
        
        /// <summary>
        ///  Order created date
        /// </summary>
        public DateTimeOffset DateCreated { get; init; }
        
        /// <summary>
        ///  Is order preparation completed
        /// </summary>
        public bool IsPreparationCompleted { get; init; }

        /// <summary>
        ///  Shop order driver
        ///  <example>{{shopOrderDriver}}</example>
        /// </summary>
        public ShopOrderDriverContract? ShopOrderDriver { get; init; }
    }
}