#nullable enable
using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Order.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Order item creation contract
    /// </summary>
    public record OrderItemCreationContract
    {
        /// <summary>
        ///  Product id
        /// </summary>
        public string ProductId { get; set; } = string.Empty;
        
        /// <summary>
        ///  Meat options
        /// </summary>
        public List<OrderItemMeatOptionCreationContract>? MeatOptions { get; set; }
        
        /// <summary>
        ///  count
        /// </summary>
        public int Count { get; set; }

        
    }
}