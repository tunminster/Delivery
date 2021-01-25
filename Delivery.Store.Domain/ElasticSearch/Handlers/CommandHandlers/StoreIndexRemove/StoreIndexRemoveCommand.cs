using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreRemove;

namespace Delivery.Store.Domain.ElasticSearch.Handlers.CommandHandlers.StoreIndexRemove
{
    public class StoreIndexRemoveCommand
    {
        public StoreIndexRemoveCommand(StoreDeletionContract storeDeletionContract)
        {
            StoreDeletionContract = storeDeletionContract;
        }
        public StoreDeletionContract StoreDeletionContract { get; }
        
    }
}