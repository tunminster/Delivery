using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;

namespace Delivery.Store.Domain.Services.ApplicationServices.StoreCreations
{
    public class StoreCreationServiceRequest
    {
        public StoreCreationServiceRequest(StoreCreationContract storeCreationContract, StoreCreationStatusContract storeCreationStatusContract)
        {
            StoreCreationContract = storeCreationContract;
            StoreCreationStatusContract = storeCreationStatusContract;
        }
        public StoreCreationContract StoreCreationContract { get;  }
        
        public StoreCreationStatusContract StoreCreationStatusContract { get; }
    }
}