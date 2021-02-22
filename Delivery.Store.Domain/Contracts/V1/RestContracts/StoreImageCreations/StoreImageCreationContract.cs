using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Microsoft.AspNetCore.Http;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreImageCreations
{
    [DataContract]
    public class StoreImageCreationContract
    {
        [DataMember]
        public IFormFile StoreImage { get; set; }
        
        [DataMember]
        public string ContainerName { get; set; }
        
        [DataMember]
        public string StoreId { get; set; }
        
        [DataMember]
        public string StoreName { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreImage)}: {StoreImage.Format()}," +
                   $"{nameof(ContainerName)}: {ContainerName.Format()}," +
                   $"{nameof(StoreId)}: {StoreId.Format()}";
        }
    }
}