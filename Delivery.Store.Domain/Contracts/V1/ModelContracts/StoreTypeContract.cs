using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations;

namespace Delivery.Store.Domain.Contracts.V1.ModelContracts
{
    /// <summary>
    ///  A contract for store type
    /// </summary>
    [DataContract]
    public class StoreTypeContract
    {
        [DataMember]
        public string StoreTypeId { get; set; }
        
        [DataMember]
        public string StoreTypeName { get; set; }
        
        [DataMember]
        public string ImageUri { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreTypeId)}: {StoreTypeId.Format()}," +
                   $"{nameof(StoreTypeName)} : {StoreTypeName.Format()}";

        }
    }
}