using System;
using System.Collections.Generic;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Order contract
    /// </summary>
    public record OrderContract
    {
        /// <summary>
        ///  Id 
        /// </summary>
        /// <example>{{id}}</example>
        public string Id { get; set; }
        
        /// <summary>
        ///  Customer id
        /// </summary>
        /// <example>{{customerId}}</example>
        public int CustomerId { get; set; }
        
        /// <summary>
        ///  Total amount
        /// </summary>
        /// <example>{{totalAmount}}</example>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        ///  Order type
        /// </summary>
        /// <example>{{orderType}}</example>
        public OrderType OrderType { get; set; }
        
        /// <summary>
        ///  Image uri 
        /// </summary>
        /// <example>{{imageUri}}</example>
        public string ImageUri { get; set; }
        
        /// <summary>
        ///  Status
        /// </summary>
        /// <example>{{status}}</example>
        public OrderStatus Status { get; set; }
        
        /// <summary>
        ///  Store name
        /// </summary>
        /// <example>{{storeName}}</example>
        public string StoreName { get; set; }
        
        /// <summary>
        ///  Date created
        /// </summary>
        /// <example>{{dateCreated}}</example>
        public DateTimeOffset DateCreated { get; set; }
        
        /// <summary>
        ///  Delivery address
        /// </summary>
        /// <example>{{deliveryAddress}}</example>
        public string DeliveryAddress { get; set; }
        
        /// <summary>
        ///  Order items
        /// </summary>
        /// <example>{{orderItems}}</example>
        public List<OrderItemContract> OrderItems { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Id)}: {Id.Format()}," +
                   $"{nameof(CustomerId)}: {CustomerId.Format()}," +
                   $"{nameof(Status)}: {Status.Format()}," +
                   $"{nameof(TotalAmount)}: {TotalAmount.Format()}," +
                   $"{nameof(OrderType)}: {OrderType.Format()}," +
                   $"{nameof(ImageUri)}: {ImageUri.Format()}," +
                   $"{nameof(StoreName)}: {StoreName.Format()}," +
                   $"{nameof(DeliveryAddress)}: {DeliveryAddress.Format()}," +
                   $"{nameof(DateCreated)}: {DateCreated.Format()}," +
                   $"{nameof(OrderItems)} : {OrderItems.Format()}";

        }
    }
    
}