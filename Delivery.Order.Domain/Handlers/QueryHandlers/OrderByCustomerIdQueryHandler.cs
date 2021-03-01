using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Objects;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.Handlers.QueryHandlers
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
                .Include(x => x.Store)
                .Include(x => x.Address)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .OrderByDescending(x => x.DateCreated)
                .Skip(query.PageSize * (query.Page - 1))
                .Take(query.PageSize)
                .ToListAsync();

            return ConvertOrderList(orderList);
        }

        private List<OrderContract> ConvertOrderList(List<Database.Entities.Order> orders)
        {
            var orderContractList = orders.Select(x => new OrderContract
            {
                Id = x.ExternalId,
                CustomerId = x.CustomerId,
                OrderStatus = x.OrderStatus,
                TotalAmount = x.TotalAmount,
                DateCreated = x.DateCreated,
                StoreName = x.Store?.StoreName,
                DeliveryAddress = x.Address != null ? x.Address.AddressLine + ", " + x.Address.City + ", " + x.Address.PostCode : "",
                ImageUri = x.OrderItems.FirstOrDefault()?.Product.ProductImageUrl,
                OrderType =  x.OrderType,
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