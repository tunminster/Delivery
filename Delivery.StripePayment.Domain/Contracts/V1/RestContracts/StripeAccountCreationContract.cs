using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.StripePayment.Domain.Contracts.Enums;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  A contract represents to create a stripe account
    /// </summary>
    [DataContract]
    public class StripeAccountCreationContract
    {
        [DataMember]
        public StripeAccountType StripeAccountType { get; set; }
        
        [DataMember]
        public StripeCountryCode StripeCountryCode { get; set; }
        
        [DataMember]
        public bool AccountPaymentOption { get; set; }
        
        [DataMember]
        public bool AccountTransferOption { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StripeAccountType)}: {StripeAccountType.Format()}," +
                   $"{nameof(StripeCountryCode)}: {StripeCountryCode.Format()}," +
                   $"{nameof(AccountPaymentOption)}: {AccountPaymentOption.Format()}," +
                   $"{nameof(AccountTransferOption)} : {AccountTransferOption.Format()}";
        }
    }
}