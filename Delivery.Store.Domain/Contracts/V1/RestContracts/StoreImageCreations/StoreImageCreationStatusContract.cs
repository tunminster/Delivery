using System;
using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Store.Domain.Contracts.V1.RestContracts.StoreImageCreations
{
    [DataContract]
    public class StoreImageCreationStatusContract
    {
        [DataMember]
        public string ImageUri { get; set; }
        
        [DataMember]
        public DateTimeOffset DateCreated { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(ImageUri)}: {ImageUri.Format()}," +
                   $"{nameof(DateCreated)}: {DateCreated.Format()}";
        }
    }
}