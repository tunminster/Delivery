using Delivery.Database.Enums;

namespace Delivery.Managements.Domain.Contracts.V1.RestContracts.MeatOptions
{
    /// <summary>
    ///  Create meat option
    /// </summary>
    public record MeatOptionCreationContract
    {
        /// <summary>
        ///  Product id
        /// </summary>
        public string ProductId { get; init; } = string.Empty;

        /// <summary>
        ///  Meat option text or question
        /// </summary>
        public string MeatOptionText { get; init; } = string.Empty;
        
        /// <summary>
        ///  Option control type. EG checkbox or radio button
        /// </summary>
        public OptionControlType OptionControlType { get; init; }
        
        
    }
}

