using System;
using Delivery.Database.Enums;
using Delivery.User.Domain.Contracts.V1.Enums;

namespace Delivery.User.Domain.Contracts.V1.RestContracts.OnBoardings.ShopOwner
{
    /// <summary>
    ///  Show owner on boarding creation contract
    /// </summary>
    public record ShopOwnerOnBoardingCreationContract
    {
        /// <summary>
        ///  First name
        /// </summary>
        /// <example>{{firstName}}</example>
        public string FirstName { get; init; } = string.Empty;
        
        /// <summary>
        ///  Last name
        /// </summary>
        /// <example>{{lastName}}</example>
        public string LastName { get; init; } = string.Empty;
        
        /// <summary>
        ///  Email
        /// </summary>
        /// <example>{{email}}</example>
        public string Email { get; init; } = string.Empty;
        
        /// <summary>
        ///  Dob
        /// </summary>
        /// <example>{{dob}}</example>
        public string Dob { get; init; } = string.Empty;
        
        /// <summary>
        ///  Address
        /// </summary>
        /// <example>{{address}}</example>
        public OnBoardingAddressContract Address { get; init; } = new();
        
        /// <summary>
        ///  Phone number
        /// </summary>
        /// <example>{{phoneNumber}}</example>
        public string PhoneNumber { get; init; } = string.Empty;
        
        /// <summary>
        ///  Social security number
        /// </summary>
        /// <example>{{socialSecurityNumber}}</example>
        public string SocialSecurityNumber { get; init; } = string.Empty;
        
        /// <summary>
        ///  Business entity
        /// </summary>
        /// <example>{{businessEntity}}</example>
        public BusinessEntity BusinessEntity { get; init; }
        
        /// <summary>
        ///  Identity document type
        /// </summary>
        /// <example>{{identityDocumentType}}</example>
        public IdentityDocumentType IdentityDocumentType { get; init; }
        
        /// <summary>
        ///  Bank account
        /// </summary>
        /// <example>{{bankAccount}}</example>
        public OnBoardingBankAccountContract BankAccount { get; init; } = new();
    }
}