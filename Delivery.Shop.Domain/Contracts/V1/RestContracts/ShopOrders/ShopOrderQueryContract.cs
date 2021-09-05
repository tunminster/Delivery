using Delivery.Database.Enums;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders
{
    public record ShopOrderQueryContract
    {
        public OrderStatus OrderStatus { get; init; }
    }
}