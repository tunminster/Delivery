using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations
{
    /// <summary>
    ///  A contract to create a store type
    /// </summary>
    [DataContract]
    public class StoreTypeCreationContract
    {
        [DataMember]
        public string StoreTypeName { get; set; }
        
        [DataMember]
        public string ImageUri { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreTypeName)} : {StoreTypeName.Format()}," +
                   $"{nameof(ImageUri)} : {ImageUri.Format()}";

        }
    }
}