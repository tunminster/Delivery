using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.Enum;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.RestContracts.OrderDetails;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.Handlers.QueryHandlers
{
    public class OrderDetailsQueryHandler : IQueryHandler<OrderDetailsQuery, OrderDetailsContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public OrderDetailsQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<OrderDetailsContract> Handle(OrderDetailsQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var order = databaseContext.Orders
                .Include(x => x.Store)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefault(x => x.ExternalId == query.OrderId);

            var address = databaseContext.Addresses.FirstOrDefault(x => x.Id == order.AddressId);

            var currentDateTime = TimeZoneInfo.ConvertTime (DateTimeOffset.Now,
                TimeZoneInfo.FindSystemTimeZoneById("Greenwich Mean Time"));
            
            currentDateTime = GetCurrentDateTime(query, currentDateTime);

            var orderDetails = ConvertOrderDetails(order, currentDateTime,
                address);


            return orderDetails;
        }

        private static DateTimeOffset GetCurrentDateTime(OrderDetailsQuery query, DateTimeOffset currentDateTime)
        {
            if (query.DeliveryTimeZone == DeliveryTimeZone.Est)
            {
                currentDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            }
            else if (query.DeliveryTimeZone == DeliveryTimeZone.Cst)
            {
                currentDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
            }
            else if (query.DeliveryTimeZone == DeliveryTimeZone.Mst)
            {
                currentDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
            }
            else if (query.DeliveryTimeZone == DeliveryTimeZone.Pst)
            {
                currentDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
            }
            else if (query.DeliveryTimeZone == DeliveryTimeZone.Ast)
            {
                currentDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Alaska Standard Time"));
            }
            else if (query.DeliveryTimeZone == DeliveryTimeZone.Hst)
            {
                currentDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("Hawaii-Aleutian Standard Time"));
            }

            return currentDateTime;
        }

        private OrderDetailsContract ConvertOrderDetails(Database.Entities.Order order, DateTimeOffset currentDateTime, Address address)
        {
            var orderDetailsContract = new OrderDetailsContract
            {
                OrderId = order.ExternalId,
                EstimatedCookingTime = $"{currentDateTime:HH:mm} {currentDateTime.AddMinutes(25):HH:mm}",
                StoreName = order.Store.StoreName,
                StoreAddress = $"{order.Store.FormattedAddress}",
                DeliveryAddress = $"{address.AddressLine},{address.City}, {address.PostCode}",
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                ImageUri = order.OrderItems.FirstOrDefault()?.Product.ProductImageUrl,
                OrderItems = order.OrderItems.Select(oi => new OrderItemContract
                {
                    Count = oi.Count,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.ProductName,
                    ProductPrice = oi.Product.UnitPrice
                }).ToList()
            };

            return orderDetailsContract;
        }
    }
}