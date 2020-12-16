using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.RestContracts;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.QueryHandlers
{
    public class OrderByCustomerIdQueryHandler : IQueryHandler<OrderByCustomerIdQuery, List<OrderContract>>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public OrderByCustomerIdQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<OrderContract>> Handle(OrderByCustomerIdQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var orderList = await databaseContext.Orders.Where(x => x.CustomerId == query.CustomerId)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .ToListAsync();

            return ConvertOrderList(orderList);
        }

        private List<OrderContract> ConvertOrderList(List<Database.Entities.Order> orders)
        {
            var orderContractList = orders.Select(x => new OrderContract
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                OrderStatus = x.OrderStatus,
                TotalAmount = x.TotalAmount,
                DateCreated = x.DateCreated,
                OrderItems = x.OrderItems.Select(oi => new OrderItemContract
                {
                    Count = oi.Count,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.ProductName,
                    ProductPrice = oi.Product.UnitPrice
                }).ToList()
            }).ToList();

            return orderContractList;
        }
    }
}