using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Microsoft.AspNetCore.Http;

namespace Delivery.Product.Domain.Contracts.V1.RestContracts.ProductImageCreations
{
    [DataContract]
    public class ProductImageCreationContract
    {
        [DataMember]
        public IFormFile ProductImage { get; set; }
        
        [DataMember]
        public string ContainerName { get; set; }
        
        [DataMember]
        public string ProductId { get; set; }
        
        [DataMember]
        public string ProductName { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(ProductImage)}: {ProductImage.Format()}," +
                   $"{nameof(ContainerName)}: {ContainerName.Format()}," +
                   $"{nameof(ProductId)}: {ProductId.Format()}";
        }
    }
}