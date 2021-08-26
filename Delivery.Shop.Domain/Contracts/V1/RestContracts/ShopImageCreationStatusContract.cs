using System;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Store image creation status contract
    /// </summary>
    public class ShopImageCreationStatusContract
    {
        /// <summary>
        ///  Shop image uri
        /// </summary>
        public string ShopImageUri { get; init; } = string.Empty;
        
        /// <summary>
        ///  Date created
        /// </summary>
        public DateTimeOffset DateCreated { get; init; }
    }
}