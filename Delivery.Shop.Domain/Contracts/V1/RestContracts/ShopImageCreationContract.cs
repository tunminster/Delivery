using System;
using Microsoft.AspNetCore.Http;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts
{
    public class ShopImageCreationContract
    {
        /// <summary>
        ///  Store unique id
        /// <example>{{storeId}}</example>
        /// </summary>
        public string StoreId { get; init; } = string.Empty;

        /// <summary>
        ///  Shop name
        /// <example>{{storeName}}</example>
        /// </summary>
        public string StoreName { get; init; } = string.Empty;
        
        /// <summary>
        /// Shop main image
        /// </summary>
        public IFormFile? ShopImage { get; init; }
    }
}