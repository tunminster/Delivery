using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Nest;

namespace Delivery.Store.Domain.Contracts.V1.ModelContracts
{
    /// <summary>
    ///  A contract for store
    /// </summary>
    public record StoreContract
    {
        /// <summary>
        ///  Store id
        /// </summary>
        /// <example>{{storeId}}</example>
        public string StoreId { get; set; } = string.Empty;

        /// <summary>
        ///  Store name
        /// </summary>
        /// <example>{{storeName}}</example>
        public string StoreName { get; set; } = string.Empty;

        /// <summary>
        ///  Image uri
        /// </summary>
        /// <example>{{imageUri}}</example>
        public string ImageUri { get; set; } = string.Empty;
        
        /// <summary>
        ///  Address line 1
        /// </summary>
        /// <example>{{addressLine1}}</example>
        public string AddressLine1 { get; set; } = string.Empty;
        
        /// <summary>
        ///  Address line 2
        /// </summary>
        /// <example>{{addressLine2}}</example>
        public string AddressLine2 { get; set; } = string.Empty;
        
        /// <summary>
        ///  City
        /// </summary>
        /// <example>{{city}}</example>
        public string City { get; set; } = string.Empty;
        
        /// <summary>
        ///  County
        /// </summary>
        /// <example>{{county}}</example>
        public string County { get; set; } = string.Empty;
        
        /// <summary>
        ///  Country
        /// </summary>
        /// <example>{{country}}</example>
        public string Country { get; set; } = string.Empty;
        
        /// <summary>
        ///  PostalCode
        /// </summary>
        /// <example>{{postalCode}}</example>
        public string PostalCode { get; set; } = string.Empty;
        
        /// <summary>
        ///  Store type
        /// </summary>
        /// <example>{{storeType}}</example>
        public string StoreType { get; set; } = string.Empty;
        
        /// <summary>
        ///  Store payment account number
        /// </summary>
        /// <example>{{storePaymentAccountNumber}}</example>
        public string StorePaymentAccountNumber { get; set; } = string.Empty;

        /// <summary>
        ///  Store opening hours
        /// </summary>
        /// <example>{{storeOpeningHours}}</example>
        public List<StoreOpeningHourContract> StoreOpeningHours { get; set; } = new();
        
        /// <summary>
        ///  Location
        /// </summary>
        /// <example>{{location}}</example>
        [GeoPoint(Name = "location")]
        public GeoLocation Location { get; set; }
        
        /// <summary>
        ///  Distance
        /// </summary>
        /// <example>{{distance}}</example>
        public double Distance { get; set; }

        /// <summary>
        ///  Contact number
        /// </summary>
        /// <example>{{contactNumber}}</example>
        public string ContactNumber { get; set; } = string.Empty;
        
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreId)}: {StoreId.Format()}," +
                   $"{nameof(StoreName)}: {StoreName.Format()}," +
                   $"{nameof(ImageUri)}: {ImageUri.Format()}," +
                   $"{nameof(AddressLine1)}: {AddressLine1.Format()}," +
                   $"{nameof(AddressLine2)}: {AddressLine2.Format()}," +
                   $"{nameof(City)}: {City.Format()}," +
                   $"{nameof(County)}: {County.Format()}," +
                   $"{nameof(Country)}: {Country.Format()}," +
                   $"{nameof(PostalCode)}: {PostalCode.Format()}," +
                   $"{nameof(Location)}: {Location.Format()}," +
                   $"{nameof(StoreType)} : {StoreType.Format()}";

        }
    }
}