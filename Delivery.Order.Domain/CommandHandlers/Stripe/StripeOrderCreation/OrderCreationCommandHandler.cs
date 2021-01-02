using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Entities;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Factories;
using Delivery.Order.Domain.Contracts.RestContracts;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrder;
using Delivery.Order.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Order.Domain.CommandHandlers.Stripe.StripeOrderCreation
{
    public class OrderCreationCommandHandler : ICommandHandler<OrderCreationCommand, OrderCreationStatus>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public OrderCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<OrderCreationStatus> Handle(OrderCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var productIds = command.StripeOrderCreationContract.OrderItems.Select(x => x.ProductId).ToList();
            var products = databaseContext.Products.Where(x => productIds.Contains(x.ExternalId)).ToList();

            //var totalAmount = 0;
            
            var orderItems = new List<OrderItem>();
            
            foreach (var item in command.StripeOrderCreationContract.OrderItems)
            {
                var id = products.FirstOrDefault(x => x.ExternalId == item.ProductId)?.Id ?? throw new InvalidOperationException($"{item.ProductId} does not exist.").WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
                orderItems.Add(new OrderItem()
                {
                    ProductId = id,
                    Count = item.Count
                });

                // if (products.Count <= 0) continue;
                // var product = products.FirstOrDefault(x => x.ExternalId == item.ProductId);
                //
                // if (product == null) continue;
                // var unitPrice = product.UnitPrice;
                // totalAmount += unitPrice * item.Count;
            }

            var orderEntity = new Database.Entities.Order
            {
                ExternalId = command.OrderCreationStatus.OrderId,
                TotalAmount = command.OrderCreationStatus.TotalAmount,
                CurrencyCode = command.OrderCreationStatus.CurrencyCode,
                PaymentType = "Card",
                PaymentStatus = PaymentStatusEnum.InProgress.ToString(),
                OrderStatus = OrderStatusEnum.InProgress.ToString(),
                Description = string.Empty,
                OrderItems = orderItems,
                DateCreated = DateTime.UtcNow,
                CustomerId = command.StripeOrderCreationContract.CustomerId,
                AddressId = command.StripeOrderCreationContract.ShippingAddressId,
                IsDeleted = false,
                InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail,
                InsertionDateTime = DateTimeOffset.UtcNow
            };

            await databaseContext.AddAsync(orderEntity);
            await databaseContext.SaveChangesAsync();

            var orderCreationStatus = command.OrderCreationStatus;
            orderCreationStatus.CreatedDateTime = orderEntity.InsertionDateTime;  

            return orderCreationStatus;
        }
    }
}