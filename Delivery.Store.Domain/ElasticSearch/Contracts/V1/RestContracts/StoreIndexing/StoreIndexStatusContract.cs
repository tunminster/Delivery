using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.ElasticSearch.Contracts.V1.RestContracts.StoreIndexing
{
    /// <summary>
    ///  A contract to return status of a store indexing
    /// </summary>
    [DataContract]
    public class StoreIndexStatusContract
    {
        [DataMember]
        public bool Status { get; set; }
        
        [DataMember]
        public DateTimeOffset InsertionDateTime { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(Status)}: {Status.Format()}," +
                   $"{nameof(InsertionDateTime)} : {InsertionDateTime.Format()}";

        }
    }
}