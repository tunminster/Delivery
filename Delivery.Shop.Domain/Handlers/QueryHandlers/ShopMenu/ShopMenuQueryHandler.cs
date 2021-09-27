using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopMenu;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile;
using Delivery.Shop.Domain.Converters.ShopMenu;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.QueryHandlers.ShopMenu
{
    public record ShopMenuQuery : IQuery<List<ShopMenuStatusContract>>;
    public class ShopMenuQueryHandler : IQueryHandler<ShopMenuQuery, List<ShopMenuStatusContract>>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopMenuQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<ShopMenuStatusContract>> Handle(ShopMenuQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail 
                            ?? throw new InvalidOperationException($"Expected authenticated user email.");
            var storeUser = await databaseContext.StoreUsers.SingleOrDefaultAsync(x => x.Username == userEmail);

            var products = await databaseContext.Products.Where(x => x.StoreId == storeUser.StoreId).ToListAsync();
            
            if(products == null)
            {
                return new List<ShopMenuStatusContract>();
            }

            var shopMenuStatusList = products.ConvertToContract();

            return shopMenuStatusList;
        }
    }
}