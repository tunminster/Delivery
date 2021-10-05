using Delivery.Azure.Library.NotificationHub.Contracts.Enums;

namespace Delivery.Azure.Library.NotificationHub.Clients.Interfaces
{
    public interface IShopDataContract
    {
        public PushNotificationType PushNotificationType { get; init; }
        public string StoreName { get; init; }
        public string StoreId { get; init; }
        public string OrderId { get; init; }
       
        // public OrderType OrderType { get; init; }
        //
        //
        // public OrderStatus Status { get; init; }
        //
        //
        // public List<OrderCreatedItemContract> ShopOrderItems { get; init; } = new();
        
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
        ///  Preparation time
        /// <example>{{preparationTime}}</example>
        /// </summary>
        public int PreparationTime { get; init; }
        
        /// <summary>
        ///  Pickup time
        /// <example>{{pickupTime}}</example>
        /// </summary>
        // public DateTimeOffset PickupTime { get; init; }
        //
        // /// <summary>
        // ///  Order created date
        // /// </summary>
        // public DateTimeOffset DateCreated { get; init; }
        //
        // /// <summary>
        // ///  Is order preparation completed
        // /// </summary>
        // public bool IsPreparationCompleted { get; init; }
        //
        // /// <summary>
        // ///  Order delivery address
        // /// </summary>
        // public OrderDeliveryAddressContract ShopOrderDeliveryAddress { get; init; }
    }
}