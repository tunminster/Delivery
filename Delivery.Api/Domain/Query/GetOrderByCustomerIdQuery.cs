using System;
using Delivery.Api.Models.Dto;
using Delivery.Api.QueryHandler;

namespace Delivery.Api.Domain.Query
{
    public class GetOrderByCustomerIdQuery : IQuery<OrderViewDto[]>
    {
        public int CustomerId { get; set; }
    }
}
