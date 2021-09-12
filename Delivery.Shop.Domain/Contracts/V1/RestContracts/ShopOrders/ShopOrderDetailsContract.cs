using Delivery.Database.Enums;
using Nest;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders
{
    /// <summary>
    ///  Shop order details contract
    /// </summary>
    public record ShopOrderDetailsContract
    {
        /// <summary>
        ///  Store id
        /// </summary>
        public string StoreId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Order Id
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        /// Order type
        /// <example>{{orderType}}</example>
        /// </summary>
        public OrderType OrderType { get; init; }
    }
}