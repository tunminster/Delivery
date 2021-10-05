using System;
using System.Collections.Generic;
using Delivery.Azure.Library.NotificationHub.Clients.Interfaces;
using Delivery.Azure.Library.NotificationHub.Contracts.Enums;
using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts.RestContracts.PushNotification
{
    /// <summary>
    ///  A contract to send a push notification to shop owner
    /// </summary>
    public class OrderCreatedPushNotificationContract : IDataContract
    {
        /// <summary>
        ///  Store id
        /// </summary>
        public string StoreId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Store name
        /// </summary>
        public string StoreName { get; init; } = string.Empty;

        /// <summary>
        ///  Order id
        /// </summary>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>
        ///  Push notification type
        /// </summary>
        public PushNotificationType PushNotificationType { get; init; }
        
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
        public List<OrderCreatedItemContract> ShopOrderItems { get; init; } = new();
        
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
        ///  Store address
        /// </summary>
        /// <example>{{storeAddress}}</example>
        public string StoreAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Delivery address
        /// <example>{{deliveryAddress}}</example>
        /// </summary>
        public string DeliveryAddress { get; init; } = string.Empty;
        
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
        ///  Order delivery address
        /// </summary>
        public OrderDeliveryAddressContract ShopOrderDeliveryAddress { get; init; }
    }
}