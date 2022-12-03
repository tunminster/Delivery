using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.QueryHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using Delivery.Shop.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.QueryHandlers.ShopOrders
{
    public record ShopOrderHistoryQuery : IQuery<List<ShopOrderContract>>
    {
        public string Email { get; init; } = string.Empty;
        public OrderStatus Status { get; init; }
        
        public DateTimeOffset DateFrom { get; init; }
    }
    
    public class ShopOrderHistoryQueryHandler : IQueryHandler<ShopOrderHistoryQuery, List<ShopOrderContract>>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopOrderHistoryQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<ShopOrderContract>> Handle(ShopOrderHistoryQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var storeUser = await databaseContext.StoreUsers.SingleOrDefaultAsync(x => x.Username == query.Email);

            var isOrderStatusEmpty = query.Status == OrderStatus.None;

            var orderList =
                await databaseContext.Orders.Where(x => isOrderStatusEmpty ? x.Status == OrderStatus.Completed || x.Status == OrderStatus.Rejected : x.Status == query.Status
                                                        && x.StoreId == storeUser.StoreId
                                                        && x.InsertionDateTime >= query.DateFrom)
                    .Include(x => x.Store)
                    .Include(x => x.OrderItems)
                        .ThenInclude(x => x.Product)
                    .Include(x => x.OrderItems)
                    .ThenInclude(x => x.OrderItemMeatOptions)
                    .ThenInclude(x => x.OrderItemMeatOptionValues)
                    .ToListAsync();
            
            if (orderList == null || orderList.Count == 0)
            {
                return new List<ShopOrderContract>();
            }
            
            var driverList = await databaseContext.DriverOrders
                .Where(x => orderList.Select(o => o.Id).ToList().Contains(x.OrderId))
                .Include(x => x.Driver)
                .ToListAsync();

            var shopOrderList = ShopOrderContractConverter.ConvertToContract(orderList, driverList);

            return shopOrderList;
        }
    }
}