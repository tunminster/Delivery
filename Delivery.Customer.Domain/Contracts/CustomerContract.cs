using System.Collections.Generic;
using System.Runtime.Serialization;
using Delivery.Address.Domain.Contracts;
using Delivery.Azure.Library.Core.Extensions;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Customer.Domain.Contracts
{
    [DataContract]
    public class CustomerContract
    {
        [DataMember]
        public int Id { get; set; }
        
        [DataMember]
        public string Username { get; set; }
        
        [DataMember]
        public string FirstName { get; set; }
        
        [DataMember]
        public string LastName { get; set; }
        
        [DataMember]
        public string ContactNumber { get; set; }

        [DataMember] 
        public List<AddressContract> Addresses { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Id)}: {Id.Format()}" +
                   $"{nameof(Username)}: {Username.Format()}" +
                   $"{nameof(FirstName)}: {FirstName.Format()}" +
                   $"{nameof(LastName)}: {LastName.Format()}" +
                   $"{nameof(ContactNumber)}: {ContactNumber.Format()}" +
                   $"{nameof(Addresses)}: {Addresses.Format()}";
        }
    }
}