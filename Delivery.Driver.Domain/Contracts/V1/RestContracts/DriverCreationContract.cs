using System;
using Delivery.Database.Enums;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Driver creation contract
    /// </summary>
    public record DriverCreationContract
    {
        /// <summary>
        ///  Driver full name
        /// <example>{{fullName}}</example>
        /// </summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>
        ///  Driver Email address
        /// <example>{{emailAddress}}</example>
        /// </summary>
        public string EmailAddress { get; init; } = string.Empty;
        
        /// <summary>
        /// Driver vehicle type
        /// <example>{{vehicleType}}</example>
        /// </summary>
        public VehicleType VehicleType { get; init; }

        /// <summary>
        ///  Driver's license number
        /// <example>{{drivingLicenseNumber}}</example>
        /// </summary>
        public string DrivingLicenseNumber { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver's social security number
        /// <example>{{socialSecurityNumber}}</example>
        /// </summary>
        public string SocialSecurityNumber { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver license expiry date
        /// <example>{{drivingLicenseExpiryDate}}</example>
        /// </summary>
        public DateTimeOffset DrivingLicenseExpiryDate { get; init; }

        /// <summary>
        ///  Driver bank name
        /// <example>{{bankName}}</example>
        /// </summary>
        public string BankName { get; init; } = string.Empty;

        /// <summary>
        ///  Driver bank account number
        /// <example>{{bankAccountNumber}}</example>
        /// </summary>
        public string BankAccountNumber { get; init; } = string.Empty;

        /// <summary>
        ///  Bank routing number
        /// <example>{{routingNumber}}</example>
        /// </summary>
        public string RoutingNumber { get; init; } = string.Empty;

        /// <summary>
        ///  Driver's service area that he/she intends to work.
        /// <example>{{serviceArea}}</example>
        /// </summary>
        public string ServiceArea { get; init; } = string.Empty;
        
        /// <summary>
        ///  Radius that would cover to give delivery service
        /// <example>{{radius}}</example>
        /// </summary>
        public int Radius { get; init; }
        
        /// <summary>
        ///  Service area latitude that driver wants to work
        /// <example>{{latitude}}</example>
        /// </summary>
        public double Latitude { get; set; }
        
        /// <summary>
        ///  Service area longitude that driver wants to work
        /// <example>{{longitude}}</example>
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        ///  Driver user account password
        /// <example>{{password}}</example>
        /// </summary>
        public string Password { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver user account confirm password
        /// <example>{{password}}</example>
        /// </summary>
        public string ConfirmPassword { get; init; } = string.Empty;

        /// <summary>
        ///  Address line 1
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

    }
}