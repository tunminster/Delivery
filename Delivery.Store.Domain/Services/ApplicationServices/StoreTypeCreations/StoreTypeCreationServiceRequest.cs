using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations;

namespace Delivery.Store.Domain.Services.ApplicationServices.StoreTypeCreations
{
    public class StoreTypeCreationServiceRequest
    {
        public StoreTypeCreationServiceRequest(StoreTypeCreationContract storeTypeCreationContract, StoreTypeCreationStatusContract storeTypeCreationStatusContract)
        {
            StoreTypeCreationContract = storeTypeCreationContract;
            StoreTypeCreationStatusContract = storeTypeCreationStatusContract;
        }
        public StoreTypeCreationContract StoreTypeCreationContract { get;  }
        
        public StoreTypeCreationStatusContract StoreTypeCreationStatusContract { get; }
    }
}