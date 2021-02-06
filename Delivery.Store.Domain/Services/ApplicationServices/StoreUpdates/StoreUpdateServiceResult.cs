using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate;

namespace Delivery.Store.Domain.Services.ApplicationServices.StoreUpdates
{
    public class StoreUpdateServiceResult
    {
        public StoreUpdateServiceResult(StoreUpdateStatusContract storeUpdateStatusContract)
        {
            StoreUpdateStatusContract = storeUpdateStatusContract;
        }
        public StoreUpdateStatusContract StoreUpdateStatusContract { get;  }
    }
}