using System;
using System.Runtime.Serialization;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrder
{
    /// <summary>
    ///  Order creation status contract
    /// </summary>
    public record OrderCreationStatusContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; set; }
        
        /// <summary>
        ///  Subtotal amount
        /// </summary>
        /// <example>{{subTotalAmount}}</example>
        public int SubtotalAmount { get; set; }
        
        /// <summary>
        ///  Total amount
        /// </summary>
        /// <example>{{totalAmount}}</example>
        public int TotalAmount { get; set; }
        
        /// <summary>
        ///  Customer application fee
        /// </summary>
        /// <example>{{customerApplicationFee}}</example>
        public int CustomerApplicationFee { get; set; }
        
        /// <summary>
        ///  Delivery fee
        /// </summary>
        /// <example>{{deliveryFee}}</example>
        public int DeliveryFee { get; set; }
        
        /// <summary>
        ///  Tax fee
        /// </summary>
        /// <example>{{taxFee}}</example>
        public int TaxFee { get; set; }
        
        /// <summary>
        ///  Currency code
        /// </summary>
        /// <example>{{currencyCode}}</example>
        public string CurrencyCode { get; set; }
        
        /// <summary>
        ///  Payment account number
        /// </summary>
        /// <example>{{paymentAccountNumber}}</example>
        public string PaymentAccountNumber { get; set; }
        
        /// <summary>
        ///  Stripe payment intent id
        /// </summary>
        /// <example>{{stripePaymentIntentId}}</example>
        public string StripePaymentIntentId { get; set; }
        
        /// <summary>
        ///  Created date time
        /// </summary>
        /// <example>{{createdDateTime}}</example>
        public DateTimeOffset CreatedDateTime { get; set; }
        
        /// <summary>
        ///  Business application fee
        /// </summary>
        /// <example>{{businessApplicationFee}}</example>
        public int BusinessApplicationFee { get; set; }
        
        /// <summary>
        ///  Delivery tips
        /// </summary>
        /// <example>{{deliveryTips}}</example>
        public int DeliveryTips { get; set; }
        
        /// <summary>
        ///  Promotion discount amount
        /// </summary>
        /// <example>{{promotionDiscountAmount}}</example>
        public int PromotionDiscountAmount { get; set; }
    }
}