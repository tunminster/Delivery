using Delivery.Category.Domain.Contracts;
using Delivery.Domain.QueryHandlers;

namespace Delivery.Category.Domain.QueryHandlers
{
    public class CategoryByIdQuery : IQuery<CategoryContract>
    {
        public string CategoryId { get; set; }
    }
}