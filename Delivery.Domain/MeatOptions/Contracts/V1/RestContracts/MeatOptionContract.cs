using System.Collections.Generic;
using Delivery.Database.Enums;

namespace Delivery.Domain.MeatOptions.Contracts.V1.RestContracts
{

    /// <summary>
    ///  Meat option contract
    /// </summary>
    public record MeatOptionContract
    {
        /// <summary>
        ///  ExternalId 
        /// </summary>
        public string Id { get; init; }

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

        /// <summary>
        ///  Product meat option values
        /// </summary>
        public List<MeatOptionValueContract> MeatOptionValues { get; init; } = new();
    }
}