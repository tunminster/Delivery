using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreGeoUpdate
{
    /// <summary>
    ///  A contract to receive a store geo update
    /// </summary>
    [DataContract]
    public class StoreGeoUpdateContract
    {
        [DataMember]
        public string StoreId { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreId)} : {StoreId.Format()}";

        }
    }
}