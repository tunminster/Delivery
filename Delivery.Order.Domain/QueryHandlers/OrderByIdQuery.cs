using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.RestContracts;

namespace Delivery.Order.Domain.QueryHandlers
{
    public class OrderByIdQuery : IQuery<OrderContract>
    {
        public int OrderId { get; set; }
    }
}