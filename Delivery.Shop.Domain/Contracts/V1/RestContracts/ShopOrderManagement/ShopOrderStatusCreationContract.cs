using Delivery.Database.Enums;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement
{
    /// <summary>
    ///  Shop order creation contract to accept or reject order
    /// </summary>
    public record ShopOrderStatusCreationContract
    {
        /// <summary>
        ///  OrderId 
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Order status
        /// </summary>
        public OrderStatus OrderStatus { get; init; }
    }
}