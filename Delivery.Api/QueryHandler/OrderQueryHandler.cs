using System;
using System.Threading.Tasks;
using Delivery.Api.Domain.Query;
using Delivery.Api.Entities;

namespace Delivery.Api.QueryHandler
{
    public class OrderQueryHandler : IQueryHandler<GetOrderIdQuery, Order>
    {
        public OrderQueryHandler()
        {
        }

        public async Task<Order> Handle(GetOrderIdQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
