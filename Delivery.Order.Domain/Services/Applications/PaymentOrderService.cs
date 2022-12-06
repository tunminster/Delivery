using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Sharding.Contracts.V1;
using Delivery.Azure.Library.Sharding.Interfaces;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Order.Domain.Constants;
using Delivery.Order.Domain.Contracts.V1.MessageContracts;
using Delivery.Order.Domain.Contracts.V1.MessageContracts.CouponPayment;
using Delivery.Order.Domain.Contracts.V1.ModelContracts.Stripe;
using Delivery.Order.Domain.Contracts.V1.RestContracts.Promotion;
using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrder;
using Delivery.Order.Domain.Converters;
using Delivery.Order.Domain.Factories;
using Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderTotalAmountCreation;
using Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripePaymentIntent;
using Delivery.Order.Domain.Handlers.MessageHandlers;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Order.Domain.Services.Applications
{
    public class PaymentOrderService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public PaymentOrderService(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<PaymentIntentCreationStatusContract> ExecuteStripePaymentIntentWorkflowAsync(PaymentOrderServiceRequest paymentOrderServiceRequest)
        {
            var orderCreateStatusContract = new OrderCreationStatusContract
            {
                CurrencyCode = paymentOrderServiceRequest.CurrencyCode
            };
            
            int promotionDiscount = 0;
            if (!string.IsNullOrEmpty(paymentOrderServiceRequest.StripeOrderCreationContract.PromoCode))
            {
                var couponCode = await GetPromotionDiscountAmountAsync(paymentOrderServiceRequest);
                promotionDiscount = couponCode?.PromotionDiscountAmount ?? 0;
            }
            
            // Generate total amount
            var orderCreationStatus =
                await new StripeOrderTotalAmountCreationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .HandleAsync(new StripeOrderTotalAmountCreationCommand(paymentOrderServiceRequest.StripeOrderCreationContract,
                    orderCreateStatusContract, paymentOrderServiceRequest.StripeOrderCreationContract.PromoCode, promotionDiscount));
            
            var businessApplicationFee = ApplicationFeeGenerator.BusinessServiceFees(orderCreationStatus.SubtotalAmount,
                OrderConstant.BusinessApplicationServiceRate);

            var customerApplicationFee = orderCreationStatus.CustomerApplicationFee;
            var deliveryFee = orderCreationStatus.DeliveryFee;
            
            var applicationFeeAmount = StripeApplicationFeesAmount.CalculateStripeApplicationFeeAmount(
                orderCreationStatus.SubtotalAmount,
                orderCreationStatus.CustomerApplicationFee,
                orderCreationStatus.DeliveryFee,
                orderCreationStatus.DeliveryTips,
                orderCreationStatus.TaxFee,
                OrderConstant.BusinessApplicationServiceRate);
            
            var shardMetadataManager = serviceProvider.GetRequiredService<IShardMetadataManager>();
            var shardInformation = shardMetadataManager.GetShardInformation<ShardInformation>().FirstOrDefault(x =>
                x.Key!.ToLower() == executingRequestContextAdapter.GetShard().Key.ToLower());
            
            
            var paymentIntentCreationContract = new PaymentIntentCreationContract
            {
                PaymentMethod = "card",
                Amount = orderCreationStatus.TotalAmount,
                ApplicationFeeAmount = applicationFeeAmount,
                StoreConnectedStripeAccountId = orderCreationStatus.PaymentAccountNumber,
                OrderId = orderCreationStatus.OrderId,
                Currency = shardInformation?.Currency != null ? shardInformation.Currency.ToLower() : "usd",
                BusinessFeeAmount = businessApplicationFee,
                DriverConnectedStripeAccountId = "",
                DeliveryFeeAmount = deliveryFee,
                DeliveryTips = orderCreationStatus.DeliveryTips,
                CustomerApplicationFeeAmount = customerApplicationFee,
                TaxFeeAmount = orderCreationStatus.TaxFee,
                Subtotal = orderCreationStatus.SubtotalAmount
            };

            var paymentIntentCreationCommand = new PaymentIntentCreationCommand(paymentIntentCreationContract);
            var paymentIntentCreationStatusContract =
                await new PaymentIntentCreationCommandHandler(serviceProvider, executingRequestContextAdapter).HandleAsync(paymentIntentCreationCommand);

            orderCreationStatus.StripePaymentIntentId = paymentIntentCreationStatusContract.StripePaymentIntentId;
            
            // To persist order to order table
            await PublishOrderCreationMessageAsync(paymentOrderServiceRequest.StripeOrderCreationContract,
                orderCreationStatus);
            
            return paymentIntentCreationStatusContract;
        }

        /// <summary>
        ///  Validate promotion code whether customer is already redeemed.
        ///  If valid, returns promotion discount amount.
        ///  If not valid, returns null.
        /// </summary>
        /// <param name="paymentOrderServiceRequest"></param>
        /// <returns></returns>
        private async Task<OrderPromotionDiscountContract> GetPromotionDiscountAmountAsync(PaymentOrderServiceRequest paymentOrderServiceRequest)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.CouponCodeCustomer>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));

            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();

            var couponCodeCustomerCacheKey =
                $"Database-{executingRequestContextAdapter.GetShard().Key}-{nameof(paymentOrderServiceRequest.StripeOrderCreationContract.PromoCode).ToLowerInvariant()}-{executingRequestContextAdapter.GetAuthenticatedUser().UserEmail}";

            var couponCodeCustomerContract = await dataAccess.GetCachedItemsAsync(
                couponCodeCustomerCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.CouponCodeCustomers
                    .FirstOrDefaultAsync(x =>
                        x.PromotionCode == paymentOrderServiceRequest.StripeOrderCreationContract.PromoCode
                        && x.Username == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail)
            );
            
            // The code has been used by the customer
            if (couponCodeCustomerContract != null)
            {
                return null;
            }
            
            var couponCodeCacheKey =
                $"Database-{executingRequestContextAdapter.GetShard().Key}-{nameof(PaymentOrderService).ToLowerInvariant()}-{paymentOrderServiceRequest.StripeOrderCreationContract.PromoCode}";

            var orderPromotionDiscountContract = await dataAccess.GetCachedItemsAsync(
                couponCodeCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.CouponCodes
                    .Where(x => x.PromotionCode == paymentOrderServiceRequest.StripeOrderCreationContract.PromoCode)
                    .Select(x => x.ConvertToOrderPromotionDiscountContract())
                    .SingleAsync());
            
            return orderPromotionDiscountContract;
        }

        private async Task PublishOrderCreationMessageAsync(StripeOrderCreationContract stripeOrderCreationContract,
            OrderCreationStatusContract orderCreationStatusContract)
        {
            var orderCreationMessage = new OrderCreationMessage
            {
                PayloadIn = stripeOrderCreationContract,
                PayloadOut = orderCreationStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };

            await new OrderCreationMessagePublisher(serviceProvider).PublishAsync(orderCreationMessage);
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(PublishOrderCreationMessageAsync)} published order creation message", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
        }
    }
}