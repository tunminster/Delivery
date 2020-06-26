using System;
using Delivery.Api.Models.Dto;
using Delivery.Api.QueryHandler;

namespace Delivery.Api.Domain.Query
{
    public class CategoryByIdQuery : IQuery<CategoryDto>
    {
        public int CategoryId { get; set; }
    }
}
