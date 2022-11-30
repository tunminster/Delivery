namespace Delivery.Product.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Product meat option value contract
    /// </summary>
    public record ProductMeatOptionValueContract
    {
        /// <summary>
        ///  Meat option value id
        /// </summary>
        public int MeatOptionValueId { get; init; }

        /// <summary>
        ///  Option value text
        /// </summary>
        public string OptionValueText { get; init; } = string.Empty;
        
        /// <summary>
        ///  Additional price
        /// </summary>
        public int AdditionalPrice { get; set; }
    }
}