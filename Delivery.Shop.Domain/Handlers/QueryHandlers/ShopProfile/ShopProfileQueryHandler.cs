using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopProfile;
using Delivery.Shop.Domain.Converters.ShopProfile;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.QueryHandlers.ShopProfile
{
    public record  ShopProfileQuery : IQuery<ShopProfileContract>
    {
        public string Email { get; init; } = string.Empty;
    }
    
    public class ShopProfileQueryHandler : IQueryHandler<ShopProfileQuery, ShopProfileContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopProfileQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ShopProfileContract> Handle(ShopProfileQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var storeUser = await databaseContext.StoreUsers.SingleOrDefaultAsync(x => x.Username == query.Email);

            var store = await databaseContext.Stores
                .Include(x => x.OpeningHours)
                .SingleOrDefaultAsync(x => x.Id == storeUser.StoreId);

            var shopProfileContract = store.ConvertToContract();
            return shopProfileContract;
        }
    }
}