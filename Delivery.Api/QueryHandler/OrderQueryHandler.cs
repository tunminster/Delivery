using System;
using Delivery.Api.Domain.Query;
using Delivery.Api.Entities;

namespace Delivery.Api.QueryHandler
{
    public class OrderQueryHandler : IQueryHandler<GetOrderIdQuery, Order>
    {
        public OrderQueryHandler()
        {
        }

        public Order Handle(GetOrderIdQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
