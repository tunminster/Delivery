namespace Delivery.Domain.MeatOptions.Contracts.V1.RestContracts
{

    /// <summary>
    ///  Meat option value contract
    /// </summary>
    public record MeatOptionValueContract
    {
        /// <summary>
        ///  Id
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        ///  Meat option value id
        /// </summary>
        public string MeatOptionId { get; init; }

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