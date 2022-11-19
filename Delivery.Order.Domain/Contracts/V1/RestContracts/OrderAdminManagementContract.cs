using System;
using Microsoft.Graph;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  This contract allow to display on the admin order list page
    /// </summary>
    public record OrderAdminManagementContract : OrderManagementContract
    {
        /// <summary>
        ///  Subtotal amount that shop owner receive
        ///   minus Delivery fees, tax fees, business service fees, platform service fees
        /// </summary>
        public int SubTotalAmount { get; init; }
        
        /// <summary>
        ///  Delivery fees
        /// </summary>
        public int DeliveryFees { get; init; }
        
        /// <summary>
        ///  Update date
        /// </summary>
        public DateTimeOffset UpdateDate { get; init; }
        
        /// <summary>
        ///  Tax fees
        /// </summary>
        public int TaxFees { get; init; }
        
        /// <summary>
        ///  Business service fees
        /// </summary>
        public int BusinessServiceFees { get; init; }
        
        /// <summary>
        ///  Platform service fees
        /// </summary>
        public int PlatformServiceFees { get; init; }
        
        
        /// <summary>
        ///  Delivery requested count
        /// </summary>
        public int DeliveryRequested { get; init; }
    }
}