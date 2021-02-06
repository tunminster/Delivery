using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreUpdate
{
    public class StoreUpdateCommand
    {
        public StoreUpdateCommand(StoreUpdateContract storeUpdateContract, StoreUpdateStatusContract storeUpdateStatusContract)
        {
            StoreUpdateContract = storeUpdateContract;
            StoreUpdateStatusContract = storeUpdateStatusContract;
        }
        
        public StoreUpdateContract StoreUpdateContract { get; }
        
        public StoreUpdateStatusContract StoreUpdateStatusContract { get; }
    }
}