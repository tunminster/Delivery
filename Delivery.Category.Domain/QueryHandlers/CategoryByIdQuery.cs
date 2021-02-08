using Delivery.Category.Domain.Contracts;
using Delivery.Category.Domain.Contracts.V1.ModelContracts;
using Delivery.Domain.QueryHandlers;

namespace Delivery.Category.Domain.QueryHandlers
{
    public class CategoryByIdQuery : IQuery<CategoryContract>
    {
        public string CategoryId { get; set; }
    }
}