using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopUsers;

namespace Delivery.Shop.Domain.Converters.ShopUsers
{
    public static class ShopUserContractConverter
    {
        public static ShopUserContract ConvertToShopUserContract(this Database.Entities.StoreUser storeUser)
        {
            var shopUserContract = new ShopUserContract
            {
                StoreId = storeUser.Store.ExternalId,
                StoreName = storeUser.Store.StoreName,
                UserId = storeUser.ExternalId,
                Username = storeUser.Username,
                Approved = storeUser.Approved
            };

            return shopUserContract;
        }
    }
}