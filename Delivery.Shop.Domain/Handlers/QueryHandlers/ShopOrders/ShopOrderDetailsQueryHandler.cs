using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using Delivery.Shop.Domain.Converters;
using Microsoft.EntityFrameworkCore;
using ServiceStack;

namespace Delivery.Shop.Domain.Handlers.QueryHandlers.ShopOrders
{
    public record ShopOrderDetailsQuery : IQuery<ShopOrderContract>
    {
        public string Email { get; init; } = string.Empty;
        public string OrderId { get; init; } = string.Empty;
    }
    public class ShopOrderDetailsQueryHandler : IQueryHandler<ShopOrderDetailsQuery, ShopOrderContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopOrderDetailsQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopOrderContract> Handle(ShopOrderDetailsQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var storeUser = await databaseContext.StoreUsers.SingleOrDefaultAsync(x => x.Username == query.Email);
            
            var order =
                await databaseContext.Orders.Where(x => x.ExternalId == query.OrderId && x.StoreId == storeUser.StoreId)
                    .Include(x => x.Store)
                    .Include(x => x.Address)
                    .Include(x => x.OrderItems)
                    .ThenInclude(x => x.Product)
                    .Include(x => x.OrderItems)
                    .ThenInclude(x => x.OrderItemMeatOptions)
                    .ThenInclude(x => x.OrderItemMeatOptionValues)
                    .SingleOrDefaultAsync();
            
            var driverOrder = await databaseContext.DriverOrders
                .Where(x => x.OrderId == order.Id)
                .Include(x => x.Driver)
                .FirstOrDefaultAsync();

            var shopOrderContract = ShopOrderContractConverter.ConvertToContract(order, driverOrder);

            return shopOrderContract;
        }
    }
}