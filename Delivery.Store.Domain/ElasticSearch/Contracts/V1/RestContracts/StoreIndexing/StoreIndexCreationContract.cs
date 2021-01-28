using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreIndexing
{
    /// <summary>
    ///  A contract to receive a store index
    /// </summary>
    [DataContract]
    public class StoreIndexCreationContract
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