using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.StripePayment.Domain.Contracts.V1.RestContracts
{
    /// <summary>
    ///  A contract represents status of an account link creation
    /// </summary>
    [DataContract]
    public class StripeAccountLinkCreationStatusContract
    {
        /// <summary>
        ///  The on-boarding user account id
        /// </summary>
        [DataMember]
        public string AccountId { get; set; }
        
        /// <summary>
        ///  a link that the on-boarding user can continue to fill information
        /// </summary>
        [DataMember]
        public string AccountLink { get; set; }
        
        /// <summary>
        ///  The expiry data of the link
        /// </summary>
        [DataMember]
        public DateTimeOffset ExpiresAt { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(AccountId)} : {AccountId.Format()}" +
                   $"{nameof(AccountLink)} : {AccountLink.Format()};";
        }
    }
}