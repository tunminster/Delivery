using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrder
{
    /// <summary>
    ///  A contract to receive an order
    /// </summary>
    public class StripeOrderCreationContract
    {
        /// <summary>
        ///  Customer Id
        /// </summary>
        /// <example>{{customerId}}</example>
        public int CustomerId { get; set; }
        
        /// <summary>
        ///  Order items
        /// </summary>
        /// <example>{{orderItems}}</example>
        public List<OrderItemCreationContract> OrderItems { get; set; } = new();
        
        /// <summary>
        ///  Shipping address id
        /// </summary>
        /// <example>{{shippingAddressId}}</example>
        public int? ShippingAddressId { get; set; }
        
        /// <summary>
        ///  Order type
        /// </summary>
        /// <example>{{orderType}}</example>
        public OrderType OrderType { get; set; }
        
        /// <summary>
        ///  Discount
        /// </summary>
        /// <example>discount</example>
        public decimal Discount { get; set; }
        
        /// <summary>
        ///  Store id
        /// </summary>
        /// <example>{{storeId}}</example>
        public string StoreId { get; set; }

        /// <summary>
        ///  Promo code
        /// </summary>
        /// <example>{{promoCode}}</example>
        public string PromoCode { get; init; } = string.Empty;
        
    }
}