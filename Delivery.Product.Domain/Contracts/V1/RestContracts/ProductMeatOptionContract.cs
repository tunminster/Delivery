using System.Collections.Generic;
using Delivery.Database.Enums;

namespace Delivery.Product.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Product meat option contract
    /// </summary>
    public record ProductMeatOptionContract
    {
        /// <summary>
        ///  Meat option id
        /// </summary>
        public int MeatOptionId { get; init; }
        
        /// <summary>
        ///  Option control EG: checkbox or radio button
        /// </summary>
        public OptionControlType OptionControl { get; init; }

        /// <summary>
        ///  Meat option text
        /// </summary>
        public string OptionText { get; init; } = string.Empty;

        /// <summary>
        ///  Product meat option values
        /// </summary>
        public List<ProductMeatOptionValueContract> ProductMeatOptionValues { get; init; } = new();

    }
}