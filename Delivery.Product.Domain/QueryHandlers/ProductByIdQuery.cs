using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.Contracts;

namespace Delivery.Product.Domain.QueryHandlers
{
    public class ProductByIdQuery : IQuery<ProductContract>
    {
        public ProductByIdQuery(int productId)
        {
            ProductId = productId;
        }
        public int ProductId { get; }
    }
}