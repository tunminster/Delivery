using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Core.Extensions.Json;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Storage.Cosmos.Accessors;
using Delivery.Azure.Library.Storage.Cosmos.Configurations;
using Delivery.Azure.Library.Storage.Cosmos.Contracts;
using Delivery.Azure.Library.Storage.Cosmos.Services;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts.DistanceMatrix;
using Delivery.Domain.Contracts.V1.RestContracts.TaxRates;
using Delivery.Domain.Factories;
using Delivery.Domain.Services;
using Delivery.Order.Domain.Constants;
using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrder;
using Delivery.Order.Domain.Factories;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

            var customerApplicationFee = ApplicationFeeGenerator.GeneratorFees(subtotalAmount);
            
            var customer = await databaseContext.Customers.Where(x =>
                x.Id == command.StripeOrderCreationContract.CustomerId)
                .Include(x => x.Addresses)
                .SingleOrDefaultAsync();

            int deliveryFee = 0;
            if (command.StripeOrderCreationContract.OrderType == OrderType.DeliverTo)
            {
                var address = customer.Addresses.FirstOrDefault(x => x.Id == command.StripeOrderCreationContract.ShippingAddressId);
                var distanceMatrix = await new DistanceMatrixService(serviceProvider, executingRequestContextAdapter)
                    .GetDistanceAsync(new DistanceMatrixRequestContract
                    {
                        DestinationLatitude = address?.Lat ?? 0,
                        DestinationLongitude = address?.Lng ?? 0,
                        SourceLatitude = store?.Latitude ?? 0,
                        SourceLongitude = store?.Longitude ?? 0
                    });
                
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"{nameof(DistanceMatrixService)} responses {distanceMatrix.ConvertToJson()}",
                    SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
            
                var distance = (distanceMatrix.Status == "OK"
                    ? distanceMatrix.Rows.FirstOrDefault()?
                        .Elements.FirstOrDefault()?.Distance?.Value : 1000) ?? 1000;
                
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>().TrackTrace($"{nameof(DistanceMatrixService)} distance value is -  {distance}",
                    SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
            
                deliveryFee = ApplicationFeeGenerator.GenerateDeliveryFees(distance);
            }
            
            var taxRate =
                await new TaxRateService(serviceProvider, executingRequestContextAdapter).GetTaxRateAsync(store?.City ?? string.Empty,
                    store?.Country ?? string.Empty);
            
            var taxFee = TaxFeeGenerator.GenerateTaxFees(subtotalAmount, taxRate);
            var totalAmount = subtotalAmount + customerApplicationFee + deliveryFee + taxFee;
            
            var businessApplicationFee = ApplicationFeeGenerator.BusinessServiceFees(subtotalAmount,
                OrderConstant.BusinessApplicationServiceRate);
            
            var orderCreationStatus =
                new OrderCreationStatusContract
                {
                    OrderId = UniqueIdFactory.UniqueExternalId(executingRequestContextAdapter.GetShard().Key.ToLowerInvariant()), 
                    CurrencyCode = command.OrderCreationStatusContract.CurrencyCode, 
                    SubtotalAmount = subtotalAmount,
                    CustomerApplicationFee = customerApplicationFee,
                    TaxFee = taxFee,
                    DeliveryFee = deliveryFee,
                    TotalAmount = totalAmount, 
                    CreatedDateTime = DateTimeOffset.UtcNow,
                    BusinessApplicationFee = businessApplicationFee,
                    PaymentAccountNumber = store?.StorePaymentAccount?.AccountNumber ?? throw new InvalidOperationException($" {command.StripeOrderCreationContract.StoreId} doesn't have payment account number.")
                        .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties())
                };
            return orderCreationStatus;
        }

        
        
        private string CacheKey { get;  }
    }
}