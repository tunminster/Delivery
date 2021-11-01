using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrderManagement;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;
using Delivery.Shop.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.QueryHandlers.ShopOrders
{
    public record ShopOrderStatusQuery : IQuery<ShopOrderStatusContract>
    {
        public ShopOrderStatusQueryContract ShopOrderStatusQueryContract { get; init; } = new();
    }
    public class ShopOrderStatusQueryHandler : IQueryHandler<ShopOrderStatusQuery, ShopOrderStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopOrderStatusQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopOrderStatusContract> Handle(ShopOrderStatusQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected an authenticated user");
            
            var storeUser = await databaseContext.StoreUsers.SingleOrDefaultAsync(x => x.Username == userEmail);

            var order = await databaseContext.Orders.SingleOrDefaultAsync(x =>
                x.ExternalId == query.ShopOrderStatusQueryContract.OrderId && x.StoreId == storeUser.StoreId);

            return order.ConvertToShopOrderStatusContract();
        }
    }
}