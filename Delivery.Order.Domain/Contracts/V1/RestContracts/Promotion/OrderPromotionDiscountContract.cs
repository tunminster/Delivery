namespace Delivery.Order.Domain.Contracts.V1.RestContracts.Promotion
{
    /// <summary>
    ///  Order promotion discount contract
    /// </summary>
    public record OrderPromotionDiscountContract
    {
        /// <summary>
        ///  Promotion id
        /// </summary>
        /// <example>{{promotionId}}</example>
        public string PromotionId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Promotion code
        /// </summary>
        /// <example>{{promotionCode}}</example>
        public string PromotionCode { get; init; } = string.Empty;
        
        /// <summary>
        ///  Promotion discount amount
        /// </summary>
        /// <example>{{promotionDiscountAmount}}</example>
        public int PromotionDiscountAmount { get; init;  }
    }
}