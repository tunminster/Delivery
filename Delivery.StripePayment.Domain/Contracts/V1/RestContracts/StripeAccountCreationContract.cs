using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.StripePayment.Domain.Contracts.Enums;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  A contract represents to create a stripe account
    /// </summary>
    public record StripeAccountCreationContract
    {
        /// <summary>
        ///  Stripe account type
        /// </summary>
        /// <example>Express</example>
        public StripeAccountType StripeAccountType { get; set; }
        
        /// <summary>
        ///  Stripe country code
        /// </summary>
        /// <example>Us</example>
        public StripeCountryCode StripeCountryCode { get; set; }

        /// <summary>
        ///  Email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        ///  Account payment option
        /// </summary>
        /// <example>true</example>
        public bool AccountPaymentOption { get; set; }
        
        /// <summary>
        ///  Account transfer option
        /// </summary>
        /// <example>true</example>
        public bool AccountTransferOption { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StripeAccountType)}: {StripeAccountType.Format()}," +
                   $"{nameof(StripeCountryCode)}: {StripeCountryCode.Format()}," +
                   $"{nameof(AccountPaymentOption)}: {AccountPaymentOption.Format()}," +
                   $"{nameof(Email)}: {Email.Format()}," +
                   $"{nameof(AccountTransferOption)} : {AccountTransferOption.Format()}";
        }
    }
}