using System;

namespace Delivery.User.Domain.Contracts.V1.RestContracts.Managements
{
    /// <summary>
    ///  User profile contract
    /// </summary>
    public record UserProfileContract
    {
        /// <summary>
        ///  ImageUri
        /// </summary>
        /// <example>{{imageUri}}</example>
        public string ImageUri { get; init; } = string.Empty;

        /// <summary>
        ///  Name
        /// </summary>
        /// <example>{{name}}</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        /// <example>{{emailAddress}}</example>
        public string EmailAddress { get; init; } = string.Empty;

        /// <summary>
        ///  Mobile number
        /// </summary>
        /// <example>{{mobileNumber}}</example>
        public string MobileNumber { get; init; } = string.Empty;
        
        /// <summary>
        ///  Date created
        /// </summary>
        /// <example>{{dateCreated}}</example>
        public DateTimeOffset DateCreated { get; init; }

        /// <summary>
        ///  Gender
        /// </summary>
        /// <example>{{gender}}</example>
        public string Gender { get; init; } = string.Empty;

        /// <summary>
        ///  Address
        /// </summary>
        /// <example>{{address}}</example>
        public string Address { get; init; } = string.Empty;

        /// <summary>
        ///  About
        /// </summary>
        /// <example>{{about}}</example>
        public string About { get; init; } = string.Empty;
    }
}