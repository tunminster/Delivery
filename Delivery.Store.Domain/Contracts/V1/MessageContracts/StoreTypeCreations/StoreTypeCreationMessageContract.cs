using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations;

namespace Delivery.Store.Domain.Contracts.V1.MessageContracts.StoreTypeCreations
{
    public class StoreTypeCreationMessageContract : AuditableResponseMessage<StoreTypeCreationContract, StoreTypeCreationStatusContract>
    {
        
    }
}