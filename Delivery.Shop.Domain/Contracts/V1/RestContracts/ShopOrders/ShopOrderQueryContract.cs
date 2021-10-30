using System;
using Delivery.Database.Enums;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders
{
    /// <summary>
    ///  Shop order query contract
    ///  This allow user to query order history by date.
    /// </summary>
    public record ShopOrderQueryContract
    {
        /// <summary>
        ///  Order status
        /// </summary>
        /// <example>{{orderStatus}}</example>
        public OrderStatus OrderStatus { get; init; }
        
        /// <summary>
        ///  Date from
        /// </summary>
        /// <example>{{dateFrom}}</example>
        public DateTimeOffset DateFrom { get; init; }
    }
}