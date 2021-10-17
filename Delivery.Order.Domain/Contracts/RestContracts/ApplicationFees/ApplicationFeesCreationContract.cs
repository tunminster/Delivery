using Delivery.Database.Enums;

namespace Delivery.Order.Domain.Contracts.RestContracts.ApplicationFees
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
    }
}