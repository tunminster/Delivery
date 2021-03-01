using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;

namespace Delivery.Order.Domain.Handlers.QueryHandlers
{
    public class OrderByIdQuery : IQuery<OrderContract>
    {
        public string OrderId { get; set; }
    }
}