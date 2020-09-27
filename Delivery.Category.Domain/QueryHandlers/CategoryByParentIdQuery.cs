using System.Collections.Generic;
using Delivery.Category.Domain.Contracts;
using Delivery.Domain.QueryHandlers;

namespace Delivery.Category.Domain.QueryHandlers
{
    public class CategoryByParentIdQuery : IQuery<List<CategoryContract>>
    {
        public int ParentId { get; set; }
        
    }
}