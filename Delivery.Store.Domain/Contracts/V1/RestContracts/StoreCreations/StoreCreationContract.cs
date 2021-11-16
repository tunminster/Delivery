using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations
{
    /// <summary>
    ///  Store creation contract
    /// </summary>
    public record StoreCreationContract
    {
        /// <summary>
        ///  Store name
        /// </summary>
        /// <example>{{storeName}}</example>
        public string StoreName { get; init; } = string.Empty;

        /// <summary>
        /// Address line 1
        /// </summary>
        /// <example>{{addressLine1}}</example>
        public string AddressLine1 { get; init; } = string.Empty;

        /// <summary>
        ///  Address line 2
        /// </summary>
        /// <example>{{addressLine2}}</example>
        public string AddressLine2 { get; init; } = string.Empty;

        /// <summary>
        ///  City
        /// </summary>
        /// <example>{{city}}</example>
        public string City { get; init; } = string.Empty;

        /// <summary>
        ///  County
        /// </summary>
        /// <example>{{county}}</example>
        public string County { get; init; } = string.Empty;

        /// <summary>
        ///  Country
        /// </summary>
        /// <example>{{country}}</example>
        public string Country { get; init; } = string.Empty;

        /// <summary>
        ///  PostalCode
        /// </summary>
        /// <example>{{postalCode}}</example>
        public string PostalCode { get; init; } = string.Empty;

        /// <summary>
        /// Store type id
        /// </summary>
        /// <example>{{postalCode}}</example>
        public string StoreTypeId { get; init; } = string.Empty;

        /// <summary>
        ///  Radius
        /// </summary>
        /// <example>20</example>
        public int Radius { get; init; }

        /// <summary>
        ///  Payment account number
        /// </summary>
        /// <example>{{paymentAccountNumber}}</example>
        public string PaymentAccountNumber { get; init; } = string.Empty;

        /// <summary>
        ///  Image uri
        /// </summary>
        /// <example>{{imageUri}}</example>
        public string ImageUri { get; set; } = string.Empty;

        /// <summary>
        ///  Store opening hours
        /// </summary>
        /// <example>{{imageUri}}</example>
        public List<StoreOpeningHourContract> StoreOpeningHours { get; init; } = new();

        /// <summary>
        ///  Store user
        /// </summary>
        /// <example>{{imageUri}}</example>
        public StoreUserCreationContract StoreUser { get; init; } = new();
    }
}