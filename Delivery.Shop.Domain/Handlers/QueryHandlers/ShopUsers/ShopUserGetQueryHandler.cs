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
    public record ShopUserGetQuery(string UserId) : IQuery<ShopUserContract>;
    
    public class ShopUserGetQueryHandler : IQueryHandler<ShopUserGetQuery, ShopUserContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public ShopUserGetQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopUserContract> Handle(ShopUserGetQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.DriverOrder>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            var shopUserCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{nameof(ShopUserGetQuery).ToLowerInvariant()}-{query.UserId}";
            
            var shopUserContract = await dataAccess.GetCachedItemsAsync(
                shopUserCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.StoreUsers
                    .Where(x => x.ExternalId == query.UserId)
                    .Include(x => x.Store)
                    .Select(x => x.ConvertToShopUserContract()).SingleAsync());

            return shopUserContract!;
        }
    }
}