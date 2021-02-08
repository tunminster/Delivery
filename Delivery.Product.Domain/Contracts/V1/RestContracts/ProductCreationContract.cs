using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Product.Domain.Contracts.V1.RestContracts
{
    [DataContract]
    public class ProductCreationContract
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string ProductName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ProductImage { get; set; }

        [DataMember]
        public string ProductImageUrl { get; set; }

        [DataMember]
        public int UnitPrice { get; set; }

        [DataMember]
        public string CategoryId { get; set; }
        
        [DataMember]
        public string StoreId { get; set; }
        
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
                   $"{nameof(UnitPrice)}: {UnitPrice.Format()};";
        }
    }
}