using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts.DistanceMatrix;
using Delivery.Domain.Services;
using Delivery.Order.Domain.Constants;
using Delivery.Order.Domain.Contracts.V1.RestContracts.ApplicationFees;
using Delivery.Order.Domain.Contracts.V1.RestContracts.Promotion;
using Delivery.Order.Domain.Converters;
using Delivery.Order.Domain.Factories;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.ApplicationFees
{
    public record ApplicationFeesCreationCommand(ApplicationFeesCreationContract ApplicationFeesCreationContract);
    
    public class ApplicationFeesCreationCommandHandler : ICommandHandler<ApplicationFeesCreationCommand, ApplicationFeesContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ApplicationFeesCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ApplicationFeesContract> HandleAsync(ApplicationFeesCreationCommand command)
        {
            var platformFee = ApplicationFeeGenerator.GeneratorFees(command.ApplicationFeesCreationContract.SubTotal);

            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var store = await databaseContext.Stores.SingleOrDefaultAsync(x =>
                x.ExternalId == command.ApplicationFeesCreationContract.StoreId);
            
            int distance = 0;
            if (command.ApplicationFeesCreationContract.CustomerLatitude == null)
            {
                

                var customer = await databaseContext.Customers.Where(x =>
                    x.ExternalId == command.ApplicationFeesCreationContract.CustomerId)
                    .Include(x => x.Addresses)
                    .SingleOrDefaultAsync();
                var address = customer.Addresses.FirstOrDefault(x => x.Disabled == false);
                var distanceMatrix = await new DistanceMatrixService(serviceProvider, executingRequestContextAdapter)
                    .GetDistanceAsync(new DistanceMatrixRequestContract
                    {
                        DestinationLatitude = address?.Lat ?? 0,
                        DestinationLongitude = address?.Lng ?? 0,
                        SourceLatitude = store?.Latitude ?? 0,
                        SourceLongitude = store?.Longitude ?? 0
                    });
                
                distance = distanceMatrix.Status == "OK"
                    ? distanceMatrix.Rows.First()
                        .Elements.First().Distance.Value : 1000;

            }
            else
            {
                var distanceMatrixContract = await new DistanceMatrixService(serviceProvider, executingRequestContextAdapter)
                    .GetDistanceAsync(new DistanceMatrixRequestContract
                    {
                        DestinationLatitude = command.ApplicationFeesCreationContract.CustomerLatitude ?? 0,
                        DestinationLongitude = command.ApplicationFeesCreationContract.CustomerLongitude ?? 0,
                        SourceLatitude = command.ApplicationFeesCreationContract.StoreLatitude ?? 0,
                        SourceLongitude = command.ApplicationFeesCreationContract.StoreLongitude ?? 0
                    });

                distance = distanceMatrixContract.Status == "OK"
                    ? distanceMatrixContract.Rows.First()
                        .Elements.First().Distance.Value : 1000;
            }
            
            var deliveryFee = ApplicationFeeGenerator.GenerateDeliveryFees(distance);
            var deliveryTips = command.ApplicationFeesCreationContract.DeliveryTips;
            if (deliveryTips == null)
            {
                deliveryTips = 0;
            }

            var promoCode = command.ApplicationFeesCreationContract.PromoCode;
            var promoDiscount = 0;

            if (!string.IsNullOrEmpty(promoCode))
            {
                var orderPromotionDiscountContract = await GetPromoDiscountAmount_Async(promoCode);
                promoDiscount = orderPromotionDiscountContract.PromotionDiscountAmount;
            }
            
            var taxRate =
                await new TaxRateService(serviceProvider, executingRequestContextAdapter).GetTaxRateAsync(
                    store?.City ?? string.Empty, store?.Country ?? string.Empty);
            
            var taxFee = TaxFeeGenerator.GenerateTaxFees(command.ApplicationFeesCreationContract.SubTotal, taxRate);

            var totalAmount = (command.ApplicationFeesCreationContract.SubTotal + platformFee + taxFee + deliveryTips) - promoDiscount;

            if (command.ApplicationFeesCreationContract.OrderType == OrderType.DeliverTo)
            {
                totalAmount += deliveryFee;
            }
            
            var applicationFeesContract = new ApplicationFeesContract
            {
                PlatformFee = platformFee,
                DeliveryFee = command.ApplicationFeesCreationContract.OrderType == OrderType.DeliverTo ? deliveryFee : 0,
                TaxFee = taxFee,
                PromotionDiscount = promoDiscount,
                DeliveryTips = deliveryTips ?? 0,
                TotalAmount = totalAmount ?? 0
            };

            return await Task.FromResult(applicationFeesContract);
        }

        private async Task<OrderPromotionDiscountContract> GetPromoDiscountAmount_Async(string promoCode)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.DriverOrder>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            var driverCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{nameof(GetPromoDiscountAmount_Async).ToLowerInvariant()}-{promoCode}";
            
            var orderPromotionCodeContract = await dataAccess.GetCachedItemsAsync(
                driverCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.CouponCodes
                    .Where(x => x.PromotionCode == promoCode)
                    .Select(x => x.ConvertToOrderPromotionDiscountContract()).SingleAsync());

            return orderPromotionCodeContract;
        }
    }
}