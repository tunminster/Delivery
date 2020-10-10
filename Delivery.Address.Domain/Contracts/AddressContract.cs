using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions;

namespace Delivery.Address.Domain.Contracts
{
    [DataContract]
    public class AddressContract
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int CustomerId { get; set; }
        [DataMember]
        public string AddressLine { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string PostCode { get; set; }
        [DataMember]
        public string Lat { get; set; }
        [DataMember]
        public string Lng { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public bool Disabled { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Id)}: {Id.Format()}," +
                   $"{nameof(CustomerId)} : {CustomerId.Format()}";

        }
    }
}