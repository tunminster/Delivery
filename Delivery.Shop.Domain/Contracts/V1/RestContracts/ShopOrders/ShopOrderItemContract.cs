using Nest;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders
{
    public record ShopOrderItemContract
    {
        public string ItemName { get; init; } = string.Empty;
        
        public double Price { get; init; }
        
        public int Count { get; init; }
        
    }
}