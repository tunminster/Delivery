using System;
using Delivery.Api.Entities;
using Delivery.Api.QueryHandler;

namespace Delivery.Api.Domain.Query
{
    public class GetOrderIdQuery : IQuery<Order>
    {
        public int OrderId { get; set; }
    }
}
