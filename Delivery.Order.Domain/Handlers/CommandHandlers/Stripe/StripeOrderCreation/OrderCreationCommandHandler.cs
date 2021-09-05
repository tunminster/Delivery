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
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;
using Delivery.Order.Domain.Enum;

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
        
        public async Task<OrderCreationStatusContract> Handle(OrderCreationCommand command)
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

            var orderEntity = new Database.Entities.Order
            {
                ExternalId = command.OrderCreationStatusContract.OrderId,
                TotalAmount = command.OrderCreationStatusContract.TotalAmount,
                CurrencyCode = command.OrderCreationStatusContract.CurrencyCode,
                PaymentType = "Card",
                PaymentStatus = PaymentStatusEnum.InProgress.ToString(),
                PaymentIntentId = command.OrderCreationStatusContract.StripePaymentIntentId,
                PaymentAccountNumber = command.OrderCreationStatusContract.PaymentAccountNumber,
                OrderStatus = OrderStatus.Preparing.ToString(),
                Status = OrderStatus.None,
                Description = string.Empty,
                OrderItems = orderItems,
                DateCreated = DateTime.UtcNow,
                CustomerId = command.StripeOrderCreationContract.CustomerId,
                AddressId = command.StripeOrderCreationContract.ShippingAddressId,
                OrderType = command.StripeOrderCreationContract.OrderType,
                IsDeleted = false,
                StoreId = store?.Id,
                InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                InsertionDateTime = DateTimeOffset.UtcNow
            };

            await databaseContext.AddAsync(orderEntity);
            await databaseContext.SaveChangesAsync();

            var orderCreationStatus = command.OrderCreationStatusContract;
            orderCreationStatus.CreatedDateTime = orderEntity.InsertionDateTime;  

            return orderCreationStatus;
        }
    }
}