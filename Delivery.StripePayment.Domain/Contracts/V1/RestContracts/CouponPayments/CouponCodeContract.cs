using System;
using Delivery.Database.Enums;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments
{
    /// <summary>
    /// Coupon code contract
    /// </summary>
    public record CouponCodeContract
    {
        /// <summary>
        ///  Promo code
        /// </summary>
        /// <example>{{promoCode}}</example>
        public string PromoCode { get; init; }
        
        /// <summary>
        ///  Coupon id
        /// </summary>
        /// <example>{{couponId}}</example>
        public string CouponId { get; set; }
        
        /// <summary>
        ///  Coupon code Type
        /// </summary>
        /// <example>{{couponCodeType}}</example>
        public CouponCodeType CouponCodeType { get; set; }
        
        /// <summary>
        ///  Redeem by
        /// </summary>
        /// <example>{{redeemBy}}</example>
        public DateTimeOffset RedeemBy { get; set; }
        
        /// <summary>
        ///  Number of times
        /// </summary>
        /// <example>{{numberOfTimes}}</example>
        public int NumberOfTimes { get; set; }
        
        /// <summary>
        ///  Minimum order value
        /// </summary>
        /// <example>{{minimumOrderValue</example>
        public int MinimumOrderValue { get; set; }
        
        /// <summary>
        /// Discount amount
        /// </summary>
        /// <example>{{discountAmount}}</example>
        public int DiscountAmount { get; set; }
    }
}