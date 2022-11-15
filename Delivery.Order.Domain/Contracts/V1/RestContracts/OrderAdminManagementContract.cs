using System;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  This contract allow to display on the admin order list page
    /// </summary>
    public record OrderAdminManagementContract : OrderManagementContract
    {
        /// <summary>
        ///  Delivery fees
        /// </summary>
        public int DeliveryFees { get; init; }
        
        /// <summary>
        ///  Update date
        /// </summary>
        public DateTimeOffset UpdateDate { get; init; }

        /// <summary>
        ///  Delivery partner name if the order is a delivery
        /// </summary>
        public string DeliveryPartnerName { get; init; } = string.Empty;

        /// <summary>
        ///  Delivery partner id
        /// </summary>
        public string DeliveryPartnerId { get; init; } = string.Empty;
    }
}