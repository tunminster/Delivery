using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Product.Domain.Contracts.V1.RestContracts;

namespace Delivery.Product.Domain.Contracts.V1.ModelContracts
{
    public record ProductContract : ProductCreationContract
    {
        public string Id { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;
        
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