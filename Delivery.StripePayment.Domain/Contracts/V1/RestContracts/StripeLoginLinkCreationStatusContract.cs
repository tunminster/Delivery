using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  A contract represents status of stripe login link creation
    /// </summary>
    [DataContract]
    public class StripeLoginLinkCreationStatusContract
    {
        [DataMember]
        public string AccountId { get; set; }
        
        [DataMember]
        public string LoginUrl { get; set; }
        
        [DataMember]
        public DateTimeOffset Created { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}, " +
                   $"{nameof(AccountId)} : {AccountId.Format()}," +
                   $"{nameof(LoginUrl)} : {LoginUrl.Format()}," +
                   $"{nameof(Created)} : {Created.Format()};";
        }
    }
}