using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;

namespace Delivery.Store.Domain.Converters.StoreCreation
{
    public static class StoreConverter
    {
        public static Database.Entities.Store Convert(StoreCreationContract storeCreationContract)
        {
            var store = new Database.Entities.Store
            {
                StoreName = storeCreationContract.StoreName,
                ImageUri = storeCreationContract.ImageUri,
                AddressLine1 = storeCreationContract.AddressLine1,
                AddressLine2 = storeCreationContract.AddressLine2,
                City = storeCreationContract.City,
                County = storeCreationContract.County,
                Country = storeCreationContract.Country,
                StoreTypeId = storeCreationContract.StoreTypeId,
                PostalCode = storeCreationContract.PostalCode
            };
            return store;
        }
    }
}