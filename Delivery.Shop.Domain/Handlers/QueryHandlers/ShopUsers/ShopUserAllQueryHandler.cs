using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopUsers;
using Delivery.Shop.Domain.Converters.ShopUsers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.QueryHandlers.ShopUsers
{
    public record ShopUserAllQuery(int PageNumber, int PageSize) : IQuery<ShopUsersPageContract>;
    
    public class ShopUserAllQueryHandler : IQueryHandler<ShopUserAllQuery, ShopUsersPageContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ShopUserAllQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopUsersPageContract> Handle(ShopUserAllQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.DriverOrder>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            var shopUsersCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{nameof(ShopUserAllQuery).ToLowerInvariant()}-{query.PageNumber}-{query.PageSize}";
            
            var shopUserTotal = await databaseContext.StoreUsers.CountAsync();
            
            var shopUserContracts = await dataAccess.GetCachedItemsAsync(
                shopUsersCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.StoreUsers
                    .OrderByDescending(x => x.InsertionDateTime)
                    .Skip(query.PageSize * (query.PageNumber - 1))
                    .Take(query.PageSize).Select(x => x.ConvertToShopUserContract()).ToListAsync());

            var shopUsersPageContract = new ShopUsersPageContract
            {
                TotalPages = (shopUserTotal + query.PageSize - 1) / query.PageSize,
                Data = shopUserContracts
            };

            return shopUsersPageContract;
        }
    }
}