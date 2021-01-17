using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreCreation
{
    public class StoreCreationCommand
    {
        public StoreCreationCommand(StoreCreationContract storeCreationContract, StoreCreationStatusContract storeCreationStatusContract)
        {
            StoreCreationContract = storeCreationContract;
            StoreCreationStatusContract = storeCreationStatusContract;
        }
        public StoreCreationContract StoreCreationContract { get; }
        
        public StoreCreationStatusContract StoreCreationStatusContract { get;  }
    }
}