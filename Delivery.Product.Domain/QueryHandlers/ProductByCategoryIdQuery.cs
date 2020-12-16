using System.Collections.Generic;
using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.Contracts;

namespace Delivery.Product.Domain.QueryHandlers
{
    public class ProductByCategoryIdQuery : IQuery<List<ProductContract>>
    {
        public ProductByCategoryIdQuery(int categoryId)
        {
            CategoryId = categoryId;
        }
        public int CategoryId { get; }
    }
}