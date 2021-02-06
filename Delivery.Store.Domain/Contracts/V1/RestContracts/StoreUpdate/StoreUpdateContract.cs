using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate
{
    [DataContract]
    public class StoreUpdateContract : StoreCreationContract
    {
        [DataMember]
        public string StoreId { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreId)}: {StoreId.Format()};";

        }
    }
}