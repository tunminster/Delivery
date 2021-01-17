using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Domain.GeoLocations.Contracts.V1.RestContracts
{
    [DataContract]
    public class SearchGeoLocationStatusContract
    {
        [DataMember]
        public double Latitude { get; set; }
        
        [DataMember]
        public double Longitude { get; set; }
        
        [DataMember]
        public string Status { get; set; }
        
        [DataMember]
        public string FormattedAddress { get; set; }
        
        [DataMember]
        public string AddressType { get; set; }
        
        [DataMember]
        public string CompoundCode { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Latitude)} : {Latitude.Format()}," +
                   $"{nameof(Longitude)} : {Longitude.Format()}," +
                   $"{nameof(Status)} : {Status.Format()};";

        }
    }
}