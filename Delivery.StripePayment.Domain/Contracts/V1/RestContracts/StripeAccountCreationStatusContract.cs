using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.StripePayment.Domain.Contracts.Enums;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  A contract represents status of an account creation
    /// </summary>
    [DataContract]
    public class StripeAccountCreationStatusContract
    {
        [DataMember]
        public string AccountId { get; set; }
        
        [DataMember]
        public StripeAccountStatus AccountStatus { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(AccountId)} : {AccountId.Format()}" +
                   $"{nameof(AccountStatus)} : {AccountStatus.Format()};";
        }
    }
}