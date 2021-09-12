using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Factories;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;
using Delivery.Order.Domain.Factories;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderTotalAmountCreation
{
    public class StripeOrderTotalAmountCreationCommandHandler : ICommandHandler<StripeOrderTotalAmountCreationCommand, OrderCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public StripeOrderTotalAmountCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter, string cacheKey)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
            CacheKey = cacheKey;
        }
        
        public async Task<OrderCreationStatusContract> Handle(StripeOrderTotalAmountCreationCommand command)
        {

            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.Order>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));

            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            
            var productIds = command.StripeOrderCreationContract.OrderItems.Select(x => x.ProductId).ToList();

            var products = await dataAccess.GetCachedItemsAsync(CacheKey, databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.Products.Where(x => productIds.Contains(x.ExternalId)).ToListAsync());

            var store = await dataAccess.GetCachedItemsAsync(
                $"Database-{executingRequestContextAdapter.GetShard().Key}-store-{command.StripeOrderCreationContract.StoreId}-default-includes",
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.Stores.Include(x => x.StorePaymentAccount)
                    .FirstOrDefaultAsync(x => x.ExternalId == command.StripeOrderCreationContract.StoreId));
            
            var subtotalAmount = 0;

            if (products != null)
            {
                foreach (var item in command.StripeOrderCreationContract.OrderItems)
                {
                    if (products.Count <= 0) continue;
                    var product = products.FirstOrDefault(x => x.ExternalId == item.ProductId);

                    if (product == null) continue;
                    var unitPrice = product.UnitPrice;
                    subtotalAmount += unitPrice * item.Count;
                }
            }

            var applicationFee = ApplicationFeeGenerator.GeneratorFees(subtotalAmount);
            var deliveryFee = ApplicationFeeGenerator.GenerateDeliveryFees(subtotalAmount);
            
            // todo: calculate tax rate
            var taxFee = TaxFeeGenerator.GenerateTaxFees(subtotalAmount, 5);
            var totalAmount = subtotalAmount + applicationFee + deliveryFee + taxFee;
            
            var orderCreationStatus =
                new OrderCreationStatusContract
                {
                    OrderId = UniqueIdFactory.UniqueExternalId(executingRequestContextAdapter.GetShard().Key.ToLowerInvariant()), 
                    CurrencyCode = command.OrderCreationStatusContract.CurrencyCode, 
                    SubtotalAmount = subtotalAmount,
                    ApplicationFee = applicationFee,
                    TaxFee = taxFee,
                    DeliveryFee = deliveryFee,
                    TotalAmount = totalAmount, 
                    CreatedDateTime = DateTimeOffset.UtcNow,
                    PaymentAccountNumber = store?.StorePaymentAccount?.AccountNumber ?? throw new InvalidOperationException($" {command.StripeOrderCreationContract.StoreId} doesn't have payment account number.")
                        .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties())
                };
            return orderCreationStatus;
        }
        
        private string CacheKey { get;  }
    }
}