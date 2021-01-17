using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;

namespace Delivery.Store.Domain.Services.ApplicationServices.StoreCreations
{
    public class StoreCreationServiceResult
    {
        public StoreCreationServiceResult(StoreCreationStatusContract storeCreationStatusContract)
        {
            StoreCreationStatusContract = storeCreationStatusContract;
        }
        public StoreCreationStatusContract StoreCreationStatusContract { get;  }
    }
}