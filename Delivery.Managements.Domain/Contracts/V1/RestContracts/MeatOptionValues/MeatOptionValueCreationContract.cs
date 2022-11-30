namespace Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptionValues
{
    /// <summary>
    ///  Meat option value creation contract
    /// </summary>
    public record MeatOptionValueCreationContract
    {
        /// <summary>
        ///  Meat option external Id
        /// </summary>
        public string MeatOptionId { get; init; } = string.Empty;

        /// <summary>
        ///  Meat option text
        /// </summary>
        public string MeatOptionValueText { get; init; } = string.Empty;
        
        /// <summary>
        ///  Price value in integer format. EG: $1 to be sent as 100
        /// </summary>
        public int AdditionalPrice { get; init; }
    }
}