using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.StripePayment.Domain.Contracts.Models
{
    /// <summary>
    /// A contract represents to stripe account connect
    /// </summary>
    [DataContract]
    public class StripeAccountContract 
    {
        [DataMember]
        public string AccountId { get; set; }
        
        [DataMember]
        public string Shard { get; set; }
        
        [DataMember]
        public string Email { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(AccountId)} : {AccountId.Format()}," +
                   $"{nameof(Shard)} : {Shard.Format()}," +
                   $"{nameof(Email)} : {Email.Format()}";
        }
    }
}