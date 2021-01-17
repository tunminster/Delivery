using System.Runtime.Serialization;
using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreGeoUpdate;

namespace Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreGeoUpdates
{
    [DataContract]
    public class StoreGeoUpdateMessageContract : AuditableResponseMessage<StoreGeoUpdateContract, StoreGeoUpdateStatusContract>
    {
        
    }
}