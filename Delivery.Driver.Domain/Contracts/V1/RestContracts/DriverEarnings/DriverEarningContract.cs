using System;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverEarnings
{
    /// <summary>
    ///  Driver earnings contract
    /// </summary>
    public class DriverEarningContract
    {
        /// <summary>
        ///  Total orders
        /// </summary>
        /// <example>{{totalOrders}}</example>
        public int TotalOrders { get; init; }

        /// <summary>
        /// Date range
        /// </summary>
        /// <example>{{dateRange}}</example>
        public string DateRange { get; init; } = string.Empty;
        
        /// <summary>
        ///  Total amount
        /// </summary>
        /// <example>{{totalAmount}}</example>
        public int TotalAmount { get; init; }

    }
}