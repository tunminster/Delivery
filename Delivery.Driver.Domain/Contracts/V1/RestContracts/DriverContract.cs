using System;
using Delivery.Database.Enums;
using Nest;

namespace Delivery.Driver.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  Driver contract 
    /// </summary>
    public record DriverContract
    {
        /// <summary>
        ///  External unique id
        /// </summary>
        /// <example>{{driverId}}</example>
        public string DriverId { get; init; } = string.Empty;
        
        /// <summary>
        ///  Driver full name
        /// </summary>
        /// <example>{{fullName}}</example>
        public string FullName { get; init; } = string.Empty;
        
        /// <summary>
        ///  Email address
        /// </summary>
        /// <example>{{emailAddress}}</example>
        public string EmailAddress { get; init; } = string.Empty;
        
        /// <summary>
        ///  Vehicle type
        /// </summary>
        public VehicleType VehicleType { get; init; }

        /// <summary>
        ///  Image uri
        /// </summary>
        /// <example>{{imageUri}}</example>
        public string ImageUri { get; init; } = string.Empty;

        /// <summary>
        ///  Latitude and longitude location
        /// </summary>
        [GeoPoint(Name = "location")]
        public GeoLocation? Location { get; set; }
        
        /// <summary>
        ///  Radius that cover to work
        /// </summary>
        public int Radius { get; init; }
        
        /// <summary>
        ///  Is active
        /// </summary>
        /// <example>{{isActive}}</example>
        public bool IsActive { get; init; }
    }
}