namespace Delivery.Order.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Order item meat option value creation contract
    /// </summary>
    public record OrderItemMeatOptionValueCreationContract
    {
        /// <summary>
        ///  Meat option value id
        /// </summary>
        public int MeatOptionValueId { get; init; }
        
        /// <summary>
        ///  Option value text
        /// </summary>
        public string OptionValueText { get; init; }
    }
}