namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopUsers
{
    
    /// <summary>
    ///  Shop user contract
    /// </summary>
    public record ShopUserContract
    {
        /// <summary>
        ///  Store id
        /// </summary>
        /// <example>{{storeId}}</example>
        public string StoreId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Store name
        /// </summary>
        /// <example>{{storeName}}</example>
        public string StoreName { get; init; } = string.Empty;
        
        /// <summary>
        /// Username
        /// </summary>
        /// <example>{{username}}</example>
        public string Username { get; init; } = string.Empty;
        
        /// <summary>
        ///  Userid
        /// </summary>
        /// <example>{{userId}}</example>
        public string UserId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Approved
        /// </summary>
        /// <example>{{approved}}</example>
        public bool Approved { get; init; }
    }
}