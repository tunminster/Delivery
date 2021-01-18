using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations;

namespace Delivery.Store.Domain.Converters.StoreTypeConverters
{
    public static class StoreTypeConverter
    {
        public static Database.Entities.StoreType Convert(StoreTypeCreationContract storeTypeCreationContract)
        {
            var storeType = new Database.Entities.StoreType
            {
                StoreTypeName = storeTypeCreationContract.StoreTypeName,
                ImageUri = storeTypeCreationContract.ImageUri
            };
            return storeType;
        }
    }
}