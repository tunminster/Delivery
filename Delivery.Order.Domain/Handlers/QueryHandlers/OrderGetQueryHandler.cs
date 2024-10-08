using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts;
using Delivery.Order.Domain.Contracts.V1.RestContracts;
using Delivery.Order.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.Handlers.QueryHandlers
{
    public record OrderGetQuery(string StoreId, int PageSize, int PageNumber, string FreeTextSearch) : IQuery<OrderPagedContract>;
    
    public class OrderGetQueryHandler : IQueryHandler<OrderGetQuery, OrderPagedContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public OrderGetQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<OrderPagedContract> Handle(OrderGetQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.Order>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            
            var storeId = string.IsNullOrEmpty(query.StoreId)
                ? databaseContext.StoreUsers
                    .Where(x => x.Username == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail)
                    .Include(x => x.Store)
                    .Single()
                    .Store.ExternalId
                : query.StoreId;

            var orderCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{query.PageSize}-{query.PageNumber}-{storeId}-{nameof(OrderGetQueryHandler).ToLowerInvariant()}-{query.FreeTextSearch}";    
            
            var store = await databaseContext.Stores.SingleOrDefaultAsync(x => x.ExternalId == storeId);
            var orderTotal = await databaseContext.Orders.Where(x => x.StoreId == store.Id).CountAsync();

            if (!string.IsNullOrEmpty(query.FreeTextSearch))
            {
                var orderManagementContractList = await dataAccess.GetCachedItemsAsync(
                    orderCacheKey,
                    databaseContext.GlobalDatabaseCacheRegion,
                    async () => await databaseContext.Orders
                        .Where(x => x.StoreId == store.Id)
                        .Where(x => x.ExternalId == query.FreeTextSearch)
                        .Include(x => x.Store)
                        .Include(x => x.Customer)
                        .Include(x => x.Address)
                        .Include(x => x.OrderItems)
                        .ThenInclude(x => x.Product)
                        .OrderByDescending(x => x.InsertionDateTime)
                        .Skip(query.PageSize * (query.PageNumber - 1))
                        .Take(query.PageSize).Select(x => x.ConvertToOrderManagementContract())
                        .ToListAsync());
                
                var orderPageContract = new OrderPagedContract
                {
                    TotalPages = (orderTotal + query.PageSize - 1) / query.PageSize,
                    Data = orderManagementContractList
                };

                return orderPageContract;
            }
            
            var orderContractList = await dataAccess.GetCachedItemsAsync(
                orderCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.Orders
                    .Where(x => x.StoreId == store.Id)
                    .Include(x => x.Store)
                    .Include(x => x.Customer)
                    .Include(x => x.Address)
                    .Include(x => x.OrderItems)
                    .ThenInclude(x => x.Product)
                    .OrderByDescending(x => x.InsertionDateTime)
                    .Skip(query.PageSize * (query.PageNumber - 1))
                    .Take(query.PageSize).Select(x => x.ConvertToOrderManagementContract())
                    .ToListAsync());
                
            var pageContract = new OrderPagedContract
            {
                TotalPages = (orderTotal + query.PageSize - 1) / query.PageSize,
                Data = orderContractList
            };

            return pageContract;
        }
    }
}