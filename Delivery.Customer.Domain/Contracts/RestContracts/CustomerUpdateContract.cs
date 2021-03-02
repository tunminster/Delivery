using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Customer.Domain.Contracts.RestContracts
{
    [DataContract]
    public class CustomerUpdateContract
    {
        [DataMember]
        public int CustomerId { get; set; }
        
        [DataMember]
        public string FirstName { get; set; }
        
        [DataMember]
        public string LastName { get; set; }
        
        [DataMember]
        public string ContactNumber { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(CustomerId)}: {CustomerId.Format()}," +
                   $"{nameof(FirstName)}: {FirstName.Format()}," +
                   $"{nameof(LastName)}: {LastName.Format()}," +
                   $"{nameof(ContactNumber)}: {ContactNumber.Format()};";

        }
    }
}