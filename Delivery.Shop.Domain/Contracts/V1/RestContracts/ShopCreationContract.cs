using System;
using System.Collections.Generic;
using Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations;

namespace Delivery.Shop.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Shop creation contract
    /// </summary>
    public record ShopCreationContract
    {
        /// <summary>
        ///  Full name of shop owner
        /// <example>{{fullName}}</example>
        /// </summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>
        ///  Email address
        /// <example>{{emailAddress}}</example>
        /// </summary>
        public string EmailAddress { get; init; } = string.Empty;

        /// <summary>
        ///  Phone Number
        /// <example>{{phoneNumber}}</example>
        /// </summary>
        public string PhoneNumber { get; init; } = string.Empty;
        
        /// <summary>
        ///  Password
        /// <example>{{password}}</example>
        /// </summary>
        public string Password { get; init; } = string.Empty;

        /// <summary>
        ///  Confirm password
        /// <example>{{confirmPassword}}</example>
        /// </summary>
        public string ConfirmPassword { get; init; } = string.Empty;

        /// <summary>
        ///  Business name
        /// <example>{{businessName}}</example>
        /// </summary>
        public string BusinessName { get; init; } = string.Empty;

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
        ///  Latitude
        /// <example>{{latitude}}</example>
        /// </summary>
        public string Latitude { get; init; } = string.Empty;

        /// <summary>
        ///  Longitude
        /// <example>{{longitude}}</example>
        /// </summary>
        public string Longitude { get; init; } = string.Empty;

        /// <summary>
        ///  Store type
        /// <example>{{storeTypeId}}</example>
        /// </summary>
        public string StoreTypeId { get; init; } = string.Empty;

        /// <summary>
        ///  Image uri
        /// </summary>
        public string ImageUri { get; set; } = string.Empty;

        /// <summary>
        ///  payment account number from stripe payment
        /// </summary>
        public string PaymentAccountNumber { get; set; } = string.Empty;
        
        /// <summary>
        ///  Store opening hours
        /// </summary>
        public List<StoreOpeningHourCreationContract> StoreOpeningHours { get; set; } = new();


    }
}