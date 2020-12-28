using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.Contracts;

namespace Delivery.Product.Domain.QueryHandlers
{
    public class ProductByIdQuery : IQuery<ProductContract>
    {
        public ProductByIdQuery(string productId)
        {
            ProductId = productId;
        }
        public string ProductId { get; }
    }
}