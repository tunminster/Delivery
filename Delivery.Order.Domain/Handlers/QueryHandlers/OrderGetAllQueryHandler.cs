using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Order.Domain.Contracts.V1.RestContracts;
using Delivery.Order.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.Handlers.QueryHandlers
{
    public record OrderGetAllQuery(int PageSize, int PageNumber, string FreeTextSearch) : IQuery<OrderAdminManagementPagedContract>;
    public class OrderGetAllQueryHandler : IQueryHandler<OrderGetAllQuery, OrderAdminManagementPagedContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public OrderGetAllQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        public async Task<OrderAdminManagementPagedContract> Handle(OrderGetAllQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.Order>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();

            var orderAllCacheKey =
                $"Database-{executingRequestContextAdapter.GetShard().Key}-{query.PageSize}-{query.PageNumber}-{nameof(OrderGetAllQueryHandler).ToLowerInvariant()}-{query.FreeTextSearch}";
            
            var orderTotal = await databaseContext.Orders.CountAsync();
            
            if (!string.IsNullOrEmpty(query.FreeTextSearch))
            {
                var orderManagementContractList = await dataAccess.GetCachedItemsAsync(
                    orderAllCacheKey,
                    databaseContext.GlobalDatabaseCacheRegion,
                    async () => await databaseContext.Orders
                        .Where(x => x.ExternalId == query.FreeTextSearch)
                        .Include(x => x.Store)
                        .Include(x => x.Customer)
                        .Include(x => x.Address)
                        .Include(x => x.OrderItems)
                        .ThenInclude(x => x.Product)
                        .OrderByDescending(x => x.InsertionDateTime)
                        .Skip(query.PageSize * (query.PageNumber - 1))
                        .Take(query.PageSize).Select(x => x.ConvertToOrderAdminManagementContract())
                        .ToListAsync());
                
                var orderPageContract = new OrderAdminManagementPagedContract
                {
                    TotalPages = (orderTotal + query.PageSize - 1) / query.PageSize,
                    Data = orderManagementContractList
                };

                return orderPageContract;
            }
            
            var orderAdminManagementContractList = await dataAccess.GetCachedItemsAsync(
                orderAllCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.Orders
                    .Include(x => x.Store)
                    .Include(x => x.Customer)
                    .Include(x => x.Address)
                    .Include(x => x.OrderItems)
                    .ThenInclude(x => x.Product)
                    .OrderByDescending(x => x.InsertionDateTime)
                    .Skip(query.PageSize * (query.PageNumber - 1))
                    .Take(query.PageSize).Select(x => x.ConvertToOrderAdminManagementContract())
                    .ToListAsync());
                
            var orderAdminManagementPagedContract = new OrderAdminManagementPagedContract
            {
                TotalPages = (orderTotal + query.PageSize - 1) / query.PageSize,
                Data = orderAdminManagementContractList
            };

            return orderAdminManagementPagedContract;
        }
    }
}