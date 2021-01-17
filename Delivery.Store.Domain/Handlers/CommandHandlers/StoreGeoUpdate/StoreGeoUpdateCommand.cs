using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreGeoUpdate;

namespace Delivery.Store.Domain.Handlers.CommandHandlers.StoreGeoUpdate
{
    public class StoreGeoUpdateCommand
    {
        public StoreGeoUpdateCommand(StoreGeoUpdateContract storeGeoUpdateContract, StoreGeoUpdateStatusContract storeGeoUpdateStatusContract)
        {
            StoreGeoUpdateContract = storeGeoUpdateContract;
            StoreGeoUpdateStatusContract = storeGeoUpdateStatusContract;
        }
        
        public StoreGeoUpdateContract StoreGeoUpdateContract { get;  }
        
        public StoreGeoUpdateStatusContract StoreGeoUpdateStatusContract { get; }
    }
}