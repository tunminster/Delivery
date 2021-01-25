using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreRemove
{
    /// <summary>
    ///  A contract to receive store deletion request
    /// </summary>
    [DataContract]
    public class StoreDeletionContract
    {
        [DataMember]
        public string IndexName { get; set; }
        
        [DataMember]
        public string StoreId { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(IndexName)} : {IndexName.Format()}" +
                   $"{nameof(StoreId)} : {StoreId.Format()}";

        }
    }
}