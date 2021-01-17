using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreGeoUpdate
{
    /// <summary>
    ///  A contract to return status of a store geo update
    /// </summary>
    [DataContract]
    public class StoreGeoUpdateStatusContract
    {
        [DataMember]
        public string StoreId { get; set; }
        
        [DataMember]
        public double? Latitude { get; set; }
        
        [DataMember]
        public double? Longitude { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreId)}: {StoreId.Format()}," +
                   $"{nameof(Latitude)}: {Latitude.Format()}," +
                   $"{nameof(Longitude)} : {Longitude.Format()}";

        }
    }
}