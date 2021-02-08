using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Product.Domain.Contracts
{
    [DataContract]
    public class ProductCreationStatusContract
    {
        [DataMember]
        public string ProductId { get; set; }
        
        [DataMember]
        public string Message { get; set; }
        
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(ProductId)}: {ProductId.Format()}" +
                   $"{nameof(Message)}: {Message.Format()}";
        }
    }
}