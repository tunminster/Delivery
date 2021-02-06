using System.Runtime.Serialization;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate
{
    [DataContract]
    public class StoreUpdateStatusContract : StoreCreationStatusContract
    {
        public override string ToString()
        {
            return $"{GetType().Name}";
        }
    }
}