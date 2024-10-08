using System;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement
{
    /// <summary>
    ///  Driver request contract for an order 
    /// </summary>
    public record ShopOrderDriverRequestContract
    {
        /// <summary>
        ///  Order id
        /// <example>{{orderId}}</example>
        /// </summary>
        public string OrderId { get; init; } = string.Empty;
    }
}