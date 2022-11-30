using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Product.Domain.Contracts.V1.RestContracts;

namespace Delivery.Product.Domain.Contracts.V1.ModelContracts
{
    /// <summary>
    ///  Product contract
    /// </summary>
    public record ProductContract : ProductCreationContract
    {
        /// <summary>
        ///  External id
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        ///  Category name
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        
        /// <summary>
        ///  Product meat options
        /// </summary>
        public List<ProductMeatOptionContract>? ProductMeatOptions { get; init; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Id)}: {Id.Format()}," +
                   $"{nameof(ProductName)}: {ProductName.Format()}," +
                   $"{nameof(Description)}: {Description.Format()}," +
                   $"{nameof(ProductImage)}: {ProductImage.Format()}," +
                   $"{nameof(StoreId)}: {StoreId.Format()}," +
                   $"{nameof(ProductImageUrl)}: {ProductImageUrl.Format()}," +
                   $"{nameof(UnitPrice)}: {UnitPrice.Format()}," +
                   $"{nameof(CategoryId)}: {CategoryId.Format()}," +
                   $"{nameof(StoreId)}: {StoreId.Format()}," +
                   $"{nameof(CategoryName)}: {CategoryName.Format()}," +
                   $"{nameof(UnitPrice)}: {UnitPrice.Format()};";
        }
    }
}