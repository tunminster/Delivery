using System.Runtime.Serialization;
using Delivery.Azure.Library.Core.Extensions.Objects;

namespace Delivery.Product.Domain.Contracts.V1.RestContracts
{
    [DataContract]
    public class ProductUpdateStatusContract : ProductCreationStatusContract
    {
        public override string ToString()
        {
            return $"{GetType().Name}" +
                   $"{nameof(ProductId)}: {ProductId.Format()}" +
                   $"{nameof(InsertionDateTime)}: {InsertionDateTime.Format()}";
        }
    }
}