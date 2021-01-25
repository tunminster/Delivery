using System.Runtime.Serialization;
using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreIndexing;

namespace Delivery.Store.Domain.ElasticSearch.Contracts.V1.MessageContracts.StoreIndexing
{
    [DataContract]
    public class StoreIndexingMessageContract : AuditableResponseMessage<StoreIndexCreationContract, StoreIndexStatusContract>
    {
        
    }
}