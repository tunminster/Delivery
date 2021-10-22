using System.Collections.Generic;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder
{
    /// <summary>
    ///  Get order details contract
    /// </summary>
    public record DriverOrderDetailsContract
    {
        /// <summary>
        ///  Order id
        /// <example>{{orderId}}</example>
        /// </summary>
        public string OrderId { get; init; } = string.Empty;

        /// <summary>
        ///  Store id
        /// <example>{{storeId}}</example>
        /// </summary>
        public string StoreId { get; init; } = string.Empty;

        /// <summary>
        ///  Store name
        ///  <example>{{storeName}}</example>
        /// </summary>
        public string StoreName { get; init; } = string.Empty;

        /// <summary>
        ///  Store address
        /// <example>{{storeAddress}}</example>
        /// </summary>
        public string StoreAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Store latitude
        /// </summary>
        /// <example>{{storeLatitude}}</example>
        public double StoreLatitude { get; init; }
        
        /// <summary>
        ///  Store longitude
        /// </summary>
        /// <example>{{storeLongitude}}</example>
        public double StoreLongitude { get; init; }
        
        /// <summary>
        ///  Delivery Address
        /// <example>{{deliveryAddress}}</example>
        /// </summary>
        public string DeliveryAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Delivery latitude
        /// </summary>
        /// <example>{{deliveryLatitude}}</example>
        public double DeliveryLatitude { get; init; }
        
        /// <summary>
        ///  Delivery longitude
        /// </summary>
        /// <example>{{deliveryLongitude}}</example>
        public double DeliveryLongitude { get; init; }

        /// <summary>
        ///  Delivery Fee
        ///  <example>{{deliveryFee}}</example>
        /// </summary>
        public int DeliveryFee { get; init; }

        /// <summary>
        ///  Tips
        /// <example>{{tips}}</example>
        /// </summary>
        public int Tips { get; init; }

        /// <summary>
        ///  Total fee
        /// </summary>
        /// <example>{{totalFee}}</example>
        public int TotalAmount { get; init; }

        /// <summary>
        ///  Order items
        /// </summary>
        public List<OrderDetailsItemContract> OrderItems { get; init; } = new();

    }

    public record OrderDetailsItemContract
    {
        public string Name { get; init; } = string.Empty;
        
        public int ItemPrice { get; init; }
    }
}