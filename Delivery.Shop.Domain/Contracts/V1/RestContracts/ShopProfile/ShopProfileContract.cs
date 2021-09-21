using System.Collections.Generic;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile
{
    public record ShopProfileContract
    {
        /// <summary>
        ///  Store id
        /// </summary>
        public string StoreId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Store name
        /// </summary>
        public string StoreName { get; init; } = string.Empty;
        
        /// <summary>
        ///  Store type id
        /// </summary>
        public string StoreTypeId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Business address line 1
        /// <example>{{addressLine1}}</example>
        /// </summary>
        public string AddressLine1 { get; init; } = string.Empty;
        
        /// <summary>
        ///  Business address line 2
        /// <example>{{addressLine2}}</example>
        /// </summary>
        public string AddressLine2 { get; init; } = string.Empty;

        /// <summary>
        ///  City
        /// <example>{{city}}</example>
        /// </summary>
        public string City { get; init; } = string.Empty;

        /// <summary>
        ///  ZipCode
        /// <example>{{zipCode}}</example>
        /// </summary>
        public string ZipCode { get; init; } = string.Empty;
        
        /// <summary>
        ///  Country
        /// <example>{{county}}</example>
        /// </summary>
        public string County { get; init; } = string.Empty;

        /// <summary>
        ///  Image uri
        /// </summary>
        public string ImageUri { get; init; } = string.Empty;
        
        /// <summary>
        ///  Radius that covers for delivery
        /// <example>{{radius}}</example>
        /// </summary>
        public int Radius { get; init; } = 5;
        
        /// <summary>
        ///  Store opening hours
        /// </summary>
        public List<StoreOpeningHourCreationContract> StoreOpeningHours { get; set; } = new();
    }
}