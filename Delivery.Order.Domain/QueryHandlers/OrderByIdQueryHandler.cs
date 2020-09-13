using System.Threading.Tasks;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.RestContracts;

namespace Delivery.Order.Domain.QueryHandlers
{
    public class OrderByIdQueryHandler : IQueryHandler<OrderByIdQuery, OrderContract>
    {
        public Task<OrderContract> Handle(OrderByIdQuery query)
        {
            throw new System.NotImplementedException();
        }
    }
}