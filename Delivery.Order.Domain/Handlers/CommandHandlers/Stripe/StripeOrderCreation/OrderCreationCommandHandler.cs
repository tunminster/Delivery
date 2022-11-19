using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Order.Domain.Constants;
using Delivery.Order.Domain.Contracts.V1.MessageContracts.CouponPayment;
using Delivery.Order.Domain.Contracts.V1.RestContracts.StripeOrder;
using Delivery.Order.Domain.Enum;
using Delivery.Order.Domain.Factories;
using Delivery.Order.Domain.Handlers.MessageHandlers;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.StripeOrderCreation
{
    public class OrderCreationCommandHandler : ICommandHandler<OrderCreationCommand, OrderCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public OrderCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<OrderCreationStatusContract> HandleAsync(OrderCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var productIds = command.StripeOrderCreationContract.OrderItems.Select(x => x.ProductId).ToList();
            var products = databaseContext.Products.Where(x => productIds.Contains(x.ExternalId)).ToList();

            if (command.StripeOrderCreationContract.ShippingAddressId == 0)
            {
                command.StripeOrderCreationContract.ShippingAddressId = null;
            }

            var store = databaseContext.Stores.FirstOrDefault(x =>
                x.ExternalId == command.StripeOrderCreationContract.StoreId);
            
            var orderItems = new List<OrderItem>();
            
            foreach (var item in command.StripeOrderCreationContract.OrderItems)
            {
                var id = products.FirstOrDefault(x => x.ExternalId == item.ProductId)?.Id ?? throw new InvalidOperationException($"{item.ProductId} does not exist.").WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
                orderItems.Add(new OrderItem()
                {
                    ProductId = id,
                    Count = item.Count
                });
            }

            var businessServiceFee =
                ApplicationFeeGenerator.BusinessServiceFees(command.OrderCreationStatusContract.SubtotalAmount, OrderConstant.BusinessApplicationServiceRate);
            var orderEntity = new Database.Entities.Order
            {
                ExternalId = command.OrderCreationStatusContract.OrderId,
                SubTotal = command.OrderCreationStatusContract.SubtotalAmount,
                TotalAmount = command.OrderCreationStatusContract.TotalAmount,
                PlatformServiceFees = command.OrderCreationStatusContract.CustomerApplicationFee,
                DeliveryFees = command.OrderCreationStatusContract.DeliveryFee,
                DeliveryTips = command.OrderCreationStatusContract.DeliveryTips,
                BusinessServiceFees = businessServiceFee,
                TaxFees = command.OrderCreationStatusContract.TaxFee,
                CurrencyCode = command.OrderCreationStatusContract.CurrencyCode,
                CouponCode = command.StripeOrderCreationContract.PromoCode,
                CouponDiscountAmount = command.OrderCreationStatusContract.PromotionDiscountAmount,
                PaymentType = "Card",
                PaymentStatus = PaymentStatusEnum.InProgress.ToString(),
                PaymentStatusCode = OrderPaymentStatus.InProgress,
                PaymentIntentId = command.OrderCreationStatusContract.StripePaymentIntentId,
                PaymentAccountNumber = command.OrderCreationStatusContract.PaymentAccountNumber,
                Status = OrderStatus.None,
                Description = string.Empty,
                OrderItems = orderItems,
                DateUpdated = DateTime.UtcNow,
                CustomerId = command.StripeOrderCreationContract.CustomerId,
                AddressId = command.StripeOrderCreationContract.ShippingAddressId,
                OrderType = command.StripeOrderCreationContract.OrderType,
                StripeTransactionFees = command.OrderCreationStatusContract.StripeTransactionFees,
                BusinessTotalAmount = command.OrderCreationStatusContract.BusinessTotalAmount,
                IsDeleted = false,
                StoreId = store?.Id,
                InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                InsertionDateTime = DateTimeOffset.UtcNow
            };

            await databaseContext.AddAsync(orderEntity);
            await databaseContext.SaveChangesAsync();

            var orderCreationStatus = command.OrderCreationStatusContract;
            orderCreationStatus.CreatedDateTime = orderEntity.InsertionDateTime;  
            
            // Publish coupon payment request
            if (!string.IsNullOrEmpty(orderEntity.CouponCode) && orderEntity.CouponDiscountAmount != null &&
                orderEntity.CouponDiscountAmount > 0)
            {
                var couponPaymentCreationMessageContract = new CouponPaymentCreationMessageContract
                {
                    CouponCode = orderEntity.CouponCode,
                    DiscountAmount = orderEntity.CouponDiscountAmount.Value,
                    OrderId = orderEntity.ExternalId,
                    ShopOwnerConnectAccount = orderEntity.ShopOwnerTransferredId
                };
                
                var couponPaymentCreationMessage = new CouponPaymentCreationMessage
                {
                    PayloadIn = couponPaymentCreationMessageContract,
                    RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
                };
                
                await new CouponPaymentCreationMessagePublisher(serviceProvider).PublishAsync(couponPaymentCreationMessage);
            }

            return orderCreationStatus;
        }
    }
}