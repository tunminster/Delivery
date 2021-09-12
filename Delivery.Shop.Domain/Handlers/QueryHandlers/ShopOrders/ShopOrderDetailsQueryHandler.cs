using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopOrders;

namespace Delivery.Shop.Domain.Handlers.QueryHandlers.ShopOrders
{
    public record ShopOrderDetailsQuery : IQuery<ShopOrderDetailsContract>
    {
        
    }
    public class ShopOrderDetailsQueryHandler : IQueryHandler<ShopOrderDetailsQuery, ShopOrderDetailsContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopOrderDetailsQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopOrderDetailsContract> Handle(ShopOrderDetailsQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            throw new System.NotImplementedException();
        }
    }
}