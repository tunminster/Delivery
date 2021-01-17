using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations;

namespace Delivery.Store.Domain.Services.ApplicationServices.StoreTypeCreations
{
    public class StoreTypeCreationServiceResult
    {
        public StoreTypeCreationServiceResult(StoreTypeCreationStatusContract storeTypeCreationStatusContract)
        {
            StoreTypeCreationStatusContract = storeTypeCreationStatusContract;
        }
        public StoreTypeCreationStatusContract StoreTypeCreationStatusContract { get;  }
    }
}