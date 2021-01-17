using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreCreations
{
    /// <summary>
    ///  A contract to return status of a store creation
    /// </summary>
    [DataContract]
    public class StoreCreationStatusContract
    {
        [DataMember]
        public string StoreId { get; set; }
        
        [DataMember]
        public DateTimeOffset InsertionDateTime { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(StoreId)}: {StoreId.Format()}," +
                   $"{nameof(InsertionDateTime)} : {InsertionDateTime.Format()}";

        }
    }
}