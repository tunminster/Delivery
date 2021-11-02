using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverEarnings
{
    /// <summary>
    ///  Driver earning details contract
    /// </summary>
    public record DriverEarningDetailsContract
    {
        /// <summary>
        ///  Order id
        /// </summary>
        /// <example>{{orderId}}</example>
        public string OrderId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Order created date
        /// </summary>
        /// <example>{{orderCreatedDate}}</example>
        public DateTimeOffset OrderCreatedDate { get; init; }
        
        /// <summary>
        ///  Delivery fee
        /// </summary>
        /// <example>{{deliveryFee}}</example>
        public int DeliveryFee { get; init; }
    }
}