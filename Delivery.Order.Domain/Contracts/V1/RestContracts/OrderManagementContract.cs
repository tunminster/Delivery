using System;
using System.Collections.Generic;
using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Order management contract
    /// </summary>
    public record OrderManagementContract
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
        public string CustomerId { get; set; }
        
        /// <summary>
        ///  Customer name
        /// </summary>
        /// <example>{{customerName}</example>
        public string CustomerName { get; set; }
        
        /// <summary>
        ///  Total amount
        /// </summary>
        /// <example>{{totalAmount}}</example>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        ///  Business application fees
        /// </summary>
        /// <example>{{businessApplicationFees}}</example>
        public decimal BusinessApplicationFees { get; set; }
        
        /// <summary>
        ///  Order type
        /// </summary>
        /// <example>{{orderType}}</example>
        public OrderType OrderType { get; set; }
        
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
    }
}