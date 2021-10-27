using Delivery.Shop.Domain.Contracts.V1.Enums;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopPaymentHistory
{
    public class ShopPaymentHistoryQueryContract
    {
        /// <summary>
        ///  Year that will be used to query payment history
        /// </summary>
        /// <example>{{year}}</example>
        public int Year { get; init; }
        
        /// <summary>
        ///  Month that will be used to query payment history
        /// </summary>
        /// <example>{{month}}</example>
        public int Month { get; init; }
        
        /// <summary>
        ///  Shop payment history filter
        /// </summary>
        /// <example>{{shopPaymentHistoryFilter}}</example>
        public ShopPaymentHistoryFilter ShopPaymentHistoryFilter { get; init; }
    }
}