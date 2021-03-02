using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Customer.Domain.Contracts.RestContracts
{
    [DataContract]
    public class CustomerUpdateStatusContract
    {
        [DataMember]
        public int CustomerId { get; set; }
        
        [DataMember]
        public string Message { get; set; }
        
        [DataMember]
        public DateTimeOffset UpdateDate { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(CustomerId)}: {CustomerId.Format()}," +
                   $"{nameof(Message)}: {Message.Format()}," +
                   $"{nameof(UpdateDate)}: {UpdateDate.Format()};";

        }
    }
}