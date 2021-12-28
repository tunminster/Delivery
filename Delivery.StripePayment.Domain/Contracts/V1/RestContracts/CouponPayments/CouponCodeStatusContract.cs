namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments
{
    /// <summary>
    ///  Coupon code confirmation creation contract
    /// </summary>
    public record CouponCodeStatusContract
    {
        /// <summary>
        /// Status
        /// </summary>
        /// <example>{{status}}</example>
        public bool Status { get; init; }
        
        /// <summary>
        /// Promo code
        /// </summary>
        /// <example>{{promoCode}}</example>
        public string PromoCode { get; init; }
        
        /// <summary>
        ///  Error message if the code is not valid
        /// </summary>
        /// <example>{{message}}</example>
        public string Message { get; init; }
    }
}