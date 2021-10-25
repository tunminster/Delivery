using System;

namespace Delivery.Product.Domain.Contracts.V1.RestContracts.ProductManagement
{
    /// <summary>
    ///  Product management creation status contract
    /// </summary>
    public record ProductManagementCreationStatusContract
    {
        /// <summary>
        ///  Product id
        /// </summary>
        /// <example>{{productId}}</example>
        public string ProductId { get; set; } = string.Empty;
        
        /// <summary>
        ///  Date created
        /// </summary>
        /// <example>{{dateCreated}}</example>
        public DateTimeOffset DateCreated { get; set; }
    }
}