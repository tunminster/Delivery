using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts.RestContracts.OrderDetails
{
    /// <summary>
    ///  Order details contract
    /// </summary>
    public record OrderDetailsContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; set; }
        
        /// <summary>
        ///  Estimated cooking time
        /// </summary>
        /// <example>{{estimatedCookingTime}}</example>
        public string EstimatedCookingTime { get; set; }
        
        /// <summary>
        ///  Store name
        /// </summary>
        /// <example>{{storeName}}</example>
        public string StoreName { get; set; }
        
        /// <summary>
        ///  Store address
        /// </summary>
        /// <example>{{storeAddress}}</example>
        public string StoreAddress { get; set; }
        
        /// <summary>
        ///  Delivery address
        /// </summary>
        /// <example>{{deliveryAddress}}</example>
        public string DeliveryAddress { get; set; }
        
        /// <summary>
        ///  Order status
        /// </summary>
        /// <example>{{orderStatus}}</example>
        public string OrderStatus { get; set; }
        
        /// <summary>
        ///  Status
        /// </summary>
        /// <example>{{status}}</example>
        public OrderStatus Status { get; set; }
        
        /// <summary>
        ///  Subtotal amount
        /// </summary>
        /// <example>{{subtotalAmount}}</example>
        public int SubtotalAmount { get; set; }
        
        /// <summary>
        ///  Total amount
        /// </summary>
        /// <example>{{totalAmount}}</example>
        public int TotalAmount { get; set; }
        
        /// <summary>
        ///  Delivery fees
        /// </summary>
        /// <example>{{deliveryFees}}</example>
        public int DeliveryFees { get; set; }
        
        /// <summary>
        ///  Tax fees
        /// </summary>
        /// <example>{{taxFees}}</example>
        public int TaxFees { get; set; }
        
        /// <summary>
        ///  Application fees
        /// </summary>
        /// <example>{{applicationFees}}</example>
        public int ApplicationFees { get; set; }
        
        /// <summary>
        ///  Image uri
        /// </summary>
        /// <example>{{imageUri}}</example>
        public string ImageUri { get; set; }
        
        /// <summary>
        ///  Order type
        /// </summary>
        /// <example>{{orderType}}</example>
        public OrderType OrderType { get; set; }
        
        /// <summary>
        ///  Order items
        /// </summary>
        /// <example>{{orderItems}}</example>
        public List<OrderItemContract> OrderItems { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(OrderId)}: {OrderId.Format()}," +
                   $"{nameof(EstimatedCookingTime)}: {EstimatedCookingTime.Format()}," +
                   $"{nameof(OrderStatus)}: {OrderStatus.Format()}," +
                   $"{nameof(TotalAmount)}: {TotalAmount.Format()}," +
                   $"{nameof(StoreName)}: {StoreName.Format()}," +
                   $"{nameof(ImageUri)}: {ImageUri.Format()}," +
                   $"{nameof(StoreName)}: {StoreName.Format()}," +
                   $"{nameof(DeliveryAddress)}: {DeliveryAddress.Format()}," +
                   $"{nameof(OrderItems)} : {OrderItems.Format()}";

        }
    }
}