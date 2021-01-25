using Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreIndexing;

namespace Delivery.Store.Domain.ElasticSearch.Handlers.CommandHandlers.StoreIndexing
{
    public class StoreIndexCommand
    {
        public StoreIndexCommand(StoreIndexCreationContract storeIndexCreationContract, StoreIndexStatusContract storeIndexStatusContract)
        {
            StoreIndexCreationContract = storeIndexCreationContract;
            StoreIndexStatusContract = storeIndexStatusContract;
        }
        
        public StoreIndexCreationContract StoreIndexCreationContract { get; }
        
        public StoreIndexStatusContract StoreIndexStatusContract { get;  }
    }
}