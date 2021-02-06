using System.Runtime.Serialization;
using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate;

namespace Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreUpdate
{
    [DataContract]
    public class StoreUpdateMessage : AuditableResponseMessage<StoreUpdateContract, StoreUpdateStatusContract>
    {
        
    }
}