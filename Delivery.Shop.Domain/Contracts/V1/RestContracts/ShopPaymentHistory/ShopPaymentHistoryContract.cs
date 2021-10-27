namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopPaymentHistory
{
    /// <summary>
    ///  Shop payment history contract
    /// </summary>
    public record ShopPaymentHistoryContract
    {
        /// <summary>
        ///  Total orders
        /// </summary>
        /// <example>{{totalOrders}}</example>
        public int TotalOrders { get; init; }

        /// <summary>
        /// Date range
        /// </summary>
        /// <example>{{dateRange}}</example>
        public string DateRange { get; init; } = string.Empty;
        
        /// <summary>
        ///  Total amount
        /// </summary>
        /// <example>{{totalAmount}}</example>
        public int TotalAmount { get; init; }
    }
}