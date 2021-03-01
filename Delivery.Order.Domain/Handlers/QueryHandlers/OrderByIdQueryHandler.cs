using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.Handlers.QueryHandlers
{
    public class OrderByIdQueryHandler : IQueryHandler<OrderByIdQuery, OrderContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public OrderByIdQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<OrderContract> Handle(OrderByIdQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var order = await databaseContext.Orders.FirstOrDefaultAsync(x => x.ExternalId == query.OrderId);

            var orderContract = new OrderContract
            {
                Id = order.ExternalId,
                CustomerId = order.CustomerId,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                DateCreated = order.DateCreated,
                OrderItems = order.OrderItems.Select(oi => new OrderItemContract
                {
                    Count = oi.Count,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.ProductName,
                    ProductPrice = oi.Product.UnitPrice
                }).ToList()
            };

            return orderContract;
        }
    }
}