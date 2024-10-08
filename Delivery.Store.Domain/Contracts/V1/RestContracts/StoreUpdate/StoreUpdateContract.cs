using System.Collections.Generic;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreUpdate
{
    /// <summary>
    ///  Store update contract
    /// </summary>
    public record StoreUpdateContract 
    {
        /// <summary>
        ///  Store id
        /// <example>{{storeId}}</example>
        /// </summary>
        public string StoreId { get; set; } = string.Empty;
        
        /// <summary>
        ///  Store name
        /// <example>{{storeName}}</example>
        /// </summary>
        public string StoreName { get; init; } = string.Empty;

        /// <summary>
        /// Address line 1
        /// <example>{{addressLine1}}</example>
        /// </summary>
        public string AddressLine1 { get; init; } = string.Empty;

        /// <summary>
        ///  Address line 2
        /// <example>{{addressLine2}}</example>
        /// </summary>
        public string AddressLine2 { get; init; } = string.Empty;

        /// <summary>
        ///  City
        /// <example>{{city}}</example>
        /// </summary>
        public string City { get; init; } = string.Empty;

        /// <summary>
        ///  County
        /// <example>{{county}}</example>
        /// </summary>
        public string County { get; init; } = string.Empty;

        /// <summary>
        ///  Country
        /// <example>{{country}}</example>
        /// </summary>
        public string Country { get; init; } = string.Empty;

        /// <summary>
        ///  PostalCode
        /// <example>{{postalCode}}</example>
        /// </summary>
        public string PostalCode { get; init; } = string.Empty;

        /// <summary>
        /// Store type id
        /// <example>{{storeTypeId}}</example>
        /// </summary>
        public string StoreTypeId { get; init; } = string.Empty;

        /// <summary>
        ///  Radius
        /// <example>{{radius}}</example>
        /// </summary>
        public int Radius { get; init; }

        /// <summary>
        ///  Payment account number
        /// <example>{{paymentAccountNumber}}</example>
        /// </summary>
        public string PaymentAccountNumber { get; init; } = string.Empty;

        /// <summary>
        ///  Image uri
        /// <example>{{imageUri}}</example>
        /// </summary>
        public string ImageUri { get; set; } = string.Empty;
        
        /// <summary>
        ///  Active 
        /// </summary>
        /// <example>{{isActive}}</example>
        public bool IsActive { get; init;  }

        /// <summary>
        ///  Store opening hours
        /// <example>{{storeOpeningHours}}</example>
        /// </summary>
        public List<StoreOpeningHourContract> StoreOpeningHours { get; init; } = new();
        
    }
}