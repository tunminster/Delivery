using Delivery.Shop.Domain.Contracts.V1.RestContracts;

namespace Delivery.Shop.Domain.Converters
{
    public static class ShopContractConverter
    {
        public static Database.Entities.Store ConvertToEntity(ShopCreationContract shopCreationContract,
            ShopCreationStatusContract shopCreationStatusContract)
        {
            var store = new Database.Entities.Store
            {
                ExternalId = shopCreationStatusContract.StoreId,
                StoreName = shopCreationContract.BusinessName,
                ImageUri = shopCreationStatusContract.ImageUri,
                AddressLine1 = shopCreationContract.AddressLine1,
                AddressLine2 = shopCreationContract.AddressLine2,
                City = shopCreationContract.City,
                County = string.Empty,
                Country = shopCreationContract.Country,
                PostalCode = shopCreationContract.ZipCode
            };

            return store;
        }
    }
}