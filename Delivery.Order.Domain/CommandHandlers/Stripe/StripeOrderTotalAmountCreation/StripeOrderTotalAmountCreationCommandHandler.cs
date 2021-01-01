using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Factories;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.CommandHandlers.Stripe.StripeOrderTotalAmountCreation
{
    public class StripeOrderTotalAmountCreationCommandHandler : ICommandHandler<StripeOrderTotalAmountCreationCommand, OrderCreationStatus>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public StripeOrderTotalAmountCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, string cacheKey)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
            CacheKey = cacheKey;
        }
        
        public async Task<OrderCreationStatus> Handle(StripeOrderTotalAmountCreationCommand command)
        {

            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.Order>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));

            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            
            var productIds = command.StripeOrderCreationContract.OrderItems.Select(x => x.ProductId).ToList();

            var products = await dataAccess.GetCachedItemsAsync(CacheKey, databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.Products.Where(x => productIds.Contains(x.ExternalId)).ToListAsync());
            
            var totalAmount = 0;

            if (products != null)
            {
                foreach (var item in command.StripeOrderCreationContract.OrderItems)
                {
                    if (products.Count <= 0) continue;
                    var product = products.FirstOrDefault(x => x.ExternalId == item.ProductId);

                    if (product == null) continue;
                    var unitPrice = product.UnitPrice;
                    totalAmount += unitPrice * item.Count;
                }
            }
            
            var orderCreationStatus =
                new OrderCreationStatus{OrderId = UniqueIdFactory.UniqueExternalId(executingRequestContextAdapter.GetShard().Key.ToLowerInvariant()), CurrencyCode = command.OrderCreationStatus.CurrencyCode, TotalAmount = totalAmount, CreatedDateTime = DateTimeOffset.UtcNow};
            return orderCreationStatus;
        }
        
        private string CacheKey { get;  }
    }
}