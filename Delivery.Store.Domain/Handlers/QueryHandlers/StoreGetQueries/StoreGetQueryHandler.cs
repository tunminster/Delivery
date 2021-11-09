using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Store.Domain.Contracts.V1.ModelContracts;
using Delivery.Store.Domain.Converters.StoreConverters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Store.Domain.Handlers.QueryHandlers.StoreGetQueries
{
    public record StoreGetQuery : IQuery<StoreContract>
    {
        public string StoreId { get; init; } = string.Empty;

        public string StoreUserEmail { get; init; } = string.Empty;
    }
    
    public class StoreGetQueryHandler : IQueryHandler<StoreGetQuery, StoreContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public StoreGetQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StoreContract> Handle(StoreGetQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            if (!string.IsNullOrEmpty(query.StoreId))
            {
                var store = await databaseContext.Stores.Where(x => x.ExternalId == query.StoreId)
                    .Include(x => x.StoreType)
                    .Include(x => x.OpeningHours)
                    .Include(x => x.StorePaymentAccount)
                    .FirstOrDefaultAsync();

                return store.ConvertStoreContract();
            }

            var storeUser = await databaseContext.StoreUsers.SingleOrDefaultAsync(x =>
                x.Username == query.StoreUserEmail);
            
            var storeDetails = await databaseContext.Stores.Where(x => x.Id == storeUser.StoreId)
                .Include(x => x.StoreType)
                .Include(x => x.OpeningHours)
                .Include(x => x.StorePaymentAccount)
                .FirstOrDefaultAsync();

            return storeDetails.ConvertStoreContract();


        }
    }
}