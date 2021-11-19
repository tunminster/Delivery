using System.Collections.Generic;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.V1.RestContracts;

namespace Delivery.Order.Domain.Handlers.QueryHandlers
{
    public class OrderByCustomerIdQuery : IQuery<List<OrderContract>>
    {
        public OrderByCustomerIdQuery(int customerId, int page, int pageSize)
        {
            CustomerId = customerId;
            Page = page;
            PageSize = pageSize;
        }
        public int CustomerId { get; }
        
        public int Page { get;  }
        
        public int PageSize { get;  }
    }
}