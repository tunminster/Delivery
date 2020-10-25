using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions;

namespace Delivery.Customer.Domain.Contracts.RestContracts
{
    [DataContract]
    public class CustomerCreationContract
    {
        [DataMember]
        public string IdentityId { get; set; }

        [DataMember]
        public string Username { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(IdentityId)}: {IdentityId.Format()}," +
                   $"{nameof(Username)} : {Username.Format()}";

        }
    }
}