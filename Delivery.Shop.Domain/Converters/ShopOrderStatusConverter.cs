using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;

namespace Delivery.Shop.Domain.Converters
{
    public static class ShopOrderStatusConverter
    {
        public static ShopOrderStatusContract ConvertToShopOrderStatusContract(this Database.Entities.Order order)
        {
            var shopOrderStatusContract = new ShopOrderStatusContract
            {
                OrderId = order.ExternalId,
                OrderStatus = order.Status,
                Status = true,
                DateCreated = order.InsertionDateTime
            };

            return shopOrderStatusContract;
        }
    }
}