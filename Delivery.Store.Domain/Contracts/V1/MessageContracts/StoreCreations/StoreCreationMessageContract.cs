using System.Runtime.Serialization;
using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;

namespace Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreCreations
{
    [DataContract]
    public class StoreCreationMessageContract : AuditableResponseMessage<StoreCreationContract, StoreCreationStatusContract>
    {
        
    }
}