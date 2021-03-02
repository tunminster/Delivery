using System;
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
using TimeZoneConverter;

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

            var address = databaseContext.Addresses.FirstOrDefault(x => x.Id == (order.AddressId ?? 0));

            var currentDateTime = TimeZoneInfo.ConvertTime (DateTimeOffset.Now,
                TZConvert.GetTimeZoneInfo("Europe/London"));
            
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
                    TZConvert.GetTimeZoneInfo("America/New_York"));
            }
            else if (query.DeliveryTimeZone == DeliveryTimeZone.Cst)
            {
                currentDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now,
                    TZConvert.GetTimeZoneInfo("America/Chicago"));
            }
            else if (query.DeliveryTimeZone == DeliveryTimeZone.Mst)
            {
                currentDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now,
                    TZConvert.GetTimeZoneInfo("America/Denver"));
            }
            else if (query.DeliveryTimeZone == DeliveryTimeZone.Pst)
            {
                currentDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now,
                    TZConvert.GetTimeZoneInfo("America/Los_Angeles"));
            }
            else if (query.DeliveryTimeZone == DeliveryTimeZone.Ast)
            {
                currentDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now,
                    TZConvert.GetTimeZoneInfo("America/Anchorage"));
            }
            else if (query.DeliveryTimeZone == DeliveryTimeZone.Hst)
            {
                currentDateTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now,
                    TZConvert.GetTimeZoneInfo("Pacific/Honolulu"));
            }

            return currentDateTime;
        }

        private OrderDetailsContract ConvertOrderDetails(Database.Entities.Order order, DateTimeOffset currentDateTime, Address address)
        {
            var orderDetailsContract = new OrderDetailsContract
            {
                OrderId = order.ExternalId,
                EstimatedCookingTime = $"{currentDateTime:HH:mm} - {currentDateTime.AddMinutes(25):HH:mm}",
                StoreName = order.Store.StoreName,
                StoreAddress = $"{order.Store.FormattedAddress}",
                DeliveryAddress = address != null ? $"{address.AddressLine}{address.City}, {address.PostCode}" : "",
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                OrderType = order.OrderType,
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