using System.Collections.Generic;
using Delivery.Domain.QueryHandlers;
using Delivery.Product.Domain.Contracts.V1.ModelContracts;

namespace Delivery.Product.Domain.QueryHandlers
{
    public class ProductByCategoryIdQuery : IQuery<List<ProductContract>>
    {
        public ProductByCategoryIdQuery(string categoryId)
        {
            CategoryId = categoryId;
        }
        public string CategoryId { get; }
    }
}