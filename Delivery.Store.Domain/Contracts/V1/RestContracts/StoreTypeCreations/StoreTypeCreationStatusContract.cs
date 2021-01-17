using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreTypeCreations
{
    /// <summary>
    ///  A contract to return status of a store type creation
    /// </summary>
    [DataContract]
    public class StoreTypeCreationStatusContract
    {
        [DataMember]
        public string StoreTypeId { get; set; }
        
        [DataMember]
        public DateTimeOffset InsertionDateTime { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(InsertionDateTime)} : {InsertionDateTime.Format()}," +
                   $"{nameof(InsertionDateTime)} : {InsertionDateTime.Format()}";

        }
    }
}