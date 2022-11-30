using System.Collections.Generic;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Order item meat option creation contract
    /// </summary>
    public record OrderItemMeatOptionCreationContract
    {
        /// <summary>
        ///  Meat option id
        /// </summary>
        public int MeatOptionId { get; init; }
        
        public List<OrderItemMeatOptionValueCreationContract> MeatOptionValues { get; init; }
    }
}