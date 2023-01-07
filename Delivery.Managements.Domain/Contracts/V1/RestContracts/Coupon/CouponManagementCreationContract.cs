using System;
using Delivery.Database.Enums;

namespace Delivery.Managements.Domain.Contracts.V1.RestContracts.Coupon
{
    /// <summary>
    ///  Coupon creation contract
    /// </summary>
    public record CouponManagementCreationContract
    {

        /// <summary>
        ///  Coupon name
        /// </summary>
        public string Name { get; init; } = string.Empty;
        
        /// <summary>
        ///  Coupon id
        /// </summary>
        public string CouponId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Coupon code type
        /// </summary>
        public CouponCodeType CouponCodeType { get; init; }

        /// <summary>
        ///  Promotion code
        /// </summary>
        public string PromotionCode { get; init; } = string.Empty;
        
        /// <summary>
        ///  Number of times
        /// </summary>
        public int NumberOfTimes { get; init; }
        
        /// <summary>
        ///  Minimum order value
        /// </summary>
        public int MinimumOrderValue { get; init; }
        
        /// <summary>
        ///  Discount amount
        /// </summary>
        public int DiscountAmount { get; init; }
        
        /// <summary>
        ///  Redeem by
        /// </summary>
        public DateTimeOffset RedeemBy { get; init; }
    }
}