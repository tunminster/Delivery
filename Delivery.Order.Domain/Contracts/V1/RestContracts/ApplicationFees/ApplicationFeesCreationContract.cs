using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts.ApplicationFees
{
    /// <summary>
    ///  Request application fees by sub total
    /// </summary>
    public record ApplicationFeesCreationContract
    {
        /// <summary>
        ///  Subtotal amount
        /// <example>1500</example>
        /// </summary>
        public int SubTotal { get; init; }
        
        /// <summary>
        ///  Order type 
        /// </summary>
        public OrderType OrderType { get; init; }
        
        /// <summary>
        ///  Customer id
        /// </summary>
        /// <example>{{customerId}}</example>
        public string CustomerId { get; init; }
        
        /// <summary>
        ///  Store id
        /// </summary>
        /// <example>{{storeId}}</example>
        public string StoreId { get; init; }
        
        /// <summary>
        ///  Customer latitude to be delivered
        /// </summary>
        /// <example>{{customerLatitude}}</example>
        public double? CustomerLatitude { get; init; }
        
        /// <summary>
        ///  Customer longitude to be delivered
        /// </summary>
        /// <example>{{customerLongitude}}</example>
        public double? CustomerLongitude { get; init; }
        
        /// <summary>
        ///  Restaurant latitude 
        /// </summary>
        /// <example>{{storeLatitude}}</example>
        public double? StoreLatitude { get; init; }
        
        /// <summary>
        ///  Store longitude
        /// </summary>
        /// <example>{{storeLongitude}}</example>
        public double? StoreLongitude { get; init; }
    }
}