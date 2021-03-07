using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Product.Domain.Contracts.V1.RestContracts
{
    [DataContract]
    public class ProductUpdateContract : ProductCreationContract
    {
        [DataMember]
        public string ProductId { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(ProductId)}: {ProductId.Format()}," +
                   $"{nameof(ProductName)}: {ProductName.Format()}," +
                   $"{nameof(Description)}: {Description.Format()}," +
                   $"{nameof(ProductImage)}: {ProductImage.Format()}," +
                   $"{nameof(StoreId)}: {StoreId.Format()}," +
                   $"{nameof(ProductImageUrl)}: {ProductImageUrl.Format()}," +
                   $"{nameof(UnitPrice)}: {UnitPrice.Format()}," +
                   $"{nameof(CategoryId)}: {CategoryId.Format()}," +
                   $"{nameof(StoreId)}: {StoreId.Format()}," +
                   $"{nameof(UnitPrice)}: {UnitPrice.Format()};";
        }
    }
}