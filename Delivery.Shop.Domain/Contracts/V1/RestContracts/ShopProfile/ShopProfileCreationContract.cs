using System.Collections.Generic;
using Delivery.Database.Entities;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile
{
    /// <summary>
    ///  Shop profile creation contract
    /// </summary>
    public record ShopProfileCreationContract
    {
        /// <summary>
        ///  Store type id
        /// </summary>
        public int StoreTypeId { get; init; }
        
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
        /// <example>{{country}}</example>
        /// </summary>
        public string Country { get; init; } = string.Empty;
        
        /// <summary>
        ///  Radius that covers for delivery
        /// <example>{{radius}}</example>
        /// </summary>
        public int Radius { get; init; }
        
        /// <summary>
        ///  Store opening hours
        /// </summary>
        public List<StoreOpeningHourCreationContract> StoreOpeningHours { get; set; } = new();
    }
}