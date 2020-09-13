using System.Collections.Generic;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.RestContracts;

namespace Delivery.Order.Domain.QueryHandlers
{
    public class OrderByCustomerIdQuery : IQuery<List<OrderContract>>
    {
        public int CustomerId { get; set; }
    }
}