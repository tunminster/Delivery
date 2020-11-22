using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Order.Domain.Contracts.RestContracts
{
    [DataContract]
    public class OrderItemCreationContract
    {
        [DataMember]
        public int ProductId { get; set;}
        
        [DataMember]
        public int Count { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(ProductId)}: {ProductId.Format()}," +
                   $"{nameof(Count)} : {Count.Format()}";

        }
    }
}