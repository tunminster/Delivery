namespace Delivery.Order.Domain.Contracts.RestContracts.ApplicationFees
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
    }
}