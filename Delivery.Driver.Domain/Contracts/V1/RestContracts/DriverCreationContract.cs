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
        /// </summary>
        public string FullName { get; init; } = string.Empty;

        /// <summary>
        ///  Driver Email address
        /// </summary>
        public string EmailAddress { get; init; } = string.Empty;
        
        /// <summary>
        /// Driver vehicle type
        /// </summary>
        public VehicleType VehicleType { get; init; }

        /// <summary>
        ///  Driver's license number
        /// </summary>
        public string DrivingLicenseNumber { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver's social security number
        /// </summary>
        public string SocialSecurityNumber { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver license expiry date
        /// </summary>
        public DateTimeOffset DrivingLicenseExpiryDate { get; init; }

        /// <summary>
        ///  Driver bank name
        /// </summary>
        public string BankName { get; init; } = string.Empty;

        /// <summary>
        ///  Driver bank account number
        /// </summary>
        public string BankAccountNumber { get; init; } = string.Empty;

        /// <summary>
        ///  Bank routing number
        /// </summary>
        public string RoutingNumber { get; init; } = string.Empty;

        /// <summary>
        ///  Driver's service area that he/she intends to work.
        /// </summary>
        public string ServiceArea { get; init; } = string.Empty;

        /// <summary>
        ///  Driver user account password
        /// </summary>
        public string Password { get; init; } = string.Empty;

    }
}