using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Domain.GeoLocations.Contracts.V1.RestContracts
{
    [DataContract]
    public class SearchGeoLocationContract
    {
        [DataMember]
        public string Address { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Address)} : {Address.Format()}";

        }
    }
}