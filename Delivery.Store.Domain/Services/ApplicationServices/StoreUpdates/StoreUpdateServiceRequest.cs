using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate;

namespace Delivery.Store.Domain.Services.ApplicationServices.StoreUpdates
{
    public class StoreUpdateServiceRequest
    {
        public StoreUpdateServiceRequest(StoreUpdateContract storeUpdateContract, StoreUpdateStatusContract storeUpdateStatusContract)
        {
            StoreUpdateContract = storeUpdateContract;
            StoreUpdateStatusContract = storeUpdateStatusContract;
        }
        public StoreUpdateContract StoreUpdateContract { get;  }
        
        public StoreUpdateStatusContract StoreUpdateStatusContract { get; }
    }
}