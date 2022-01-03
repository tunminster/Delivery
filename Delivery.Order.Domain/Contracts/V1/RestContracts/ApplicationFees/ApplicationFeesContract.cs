namespace Delivery.Order.Domain.Contracts.V1.RestContracts.ApplicationFees
{
    /// <summary>
    ///  Contains application fees 
    /// </summary>
    public record ApplicationFeesContract
    {
        /// <summary>
        ///  Platform service fee to customer
        /// </summary>
        public int PlatformFee { get; init; }
        
        /// <summary>
        ///  Fees to delivery driver
        /// </summary>
        public int DeliveryFee { get; init; }
        
        /// <summary>
        ///  Tax amount of order
        /// </summary>
        public int TaxFee { get; init; }
        
        /// <summary>
        ///  Total amount of order
        /// </summary>
        public int TotalAmount { get; init; }
        
        /// <summary>
        ///  Delivery tips
        /// </summary>
        public int DeliveryTips { get; init; }
        
        /// <summary>
        ///  Promotion discount
        /// </summary>
        /// <example>{{promotionDiscount}}</example>
        public int PromotionDiscount { get; init; }
    }
}