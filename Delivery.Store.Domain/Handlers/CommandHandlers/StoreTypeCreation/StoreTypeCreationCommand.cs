using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreTypeCreation
{
    public class StoreTypeCreationCommand
    {
        public StoreTypeCreationCommand(StoreTypeCreationContract storeTypeCreationContract, StoreTypeCreationStatusContract storeTypeCreationStatusContract)
        {
            StoreTypeCreationContract = storeTypeCreationContract;
            StoreTypeCreationStatusContract = storeTypeCreationStatusContract;
        }
        
        public StoreTypeCreationContract StoreTypeCreationContract { get; }
        
        public StoreTypeCreationStatusContract StoreTypeCreationStatusContract { get;  }
    }
}