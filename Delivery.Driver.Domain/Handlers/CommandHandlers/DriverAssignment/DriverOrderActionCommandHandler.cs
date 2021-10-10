using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverPayments;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverPayments;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverPayments;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment
{
    public record DriverOrderActionCommand(DriverOrderActionContract DriverOrderActionContract);
    
    public class DriverOrderActionCommandHandler : ICommandHandler<DriverOrderActionCommand, StatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverOrderActionCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> Handle(DriverOrderActionCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var driver = await databaseContext.Drivers.SingleAsync(x =>
                x.EmailAddress == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail);

            var order = await databaseContext.Orders.SingleOrDefaultAsync(x =>
                x.ExternalId == command.DriverOrderActionContract.OrderId) ?? throw new InvalidOperationException($"Expected order by order id: {command.DriverOrderActionContract.OrderId}.");

            var driverOrder =
                await databaseContext.DriverOrders.SingleOrDefaultAsync(x =>
                    x.OrderId == order.Id && x.DriverId == driver.Id);

            if (driverOrder == null)
            {
                return new StatusContract
                {
                    Status = false,
                    DateCreated = DateTimeOffset.UtcNow
                };
            }
            
            driverOrder.Status = command.DriverOrderActionContract.DriverOrderStatus;
            driverOrder.Reason = command.DriverOrderActionContract.Reason;

            if (command.DriverOrderActionContract.DriverOrderStatus == DriverOrderStatus.InProgress)
            {
                var driverOrderInProgressMessageContract = new DriverOrderInProgressMessageContract
                {
                    PayloadIn = new EntityUpdateContract { Id = command.DriverOrderActionContract.OrderId },
                    PayloadOut = new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow },
                    RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
                };
                await new DriverOrderInProgressMessagePublisher(serviceProvider).PublishAsync(
                    driverOrderInProgressMessageContract);
                
            }

            if (command.DriverOrderActionContract.DriverOrderStatus == DriverOrderStatus.Complete)
            {
                // send message order update
                var driverOrderCompleteMessageContract = new DriverOrderCompleteMessageContract
                {
                    PayloadIn = new EntityUpdateContract { Id = command.DriverOrderActionContract.OrderId },
                    PayloadOut = new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow },
                    RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
                };

                await new DriverOrderCompleteMessagePublisher(serviceProvider).PublishAsync(
                    driverOrderCompleteMessageContract);
                
                // split payment request

                if (!string.IsNullOrEmpty(driver.PaymentAccountId))
                {
                    var driverPaymentCreationMessageContract = new DriverPaymentCreationMessageContract
                    {
                        PayloadIn = new DriverPaymentCreationContract
                            { DriverConnectAccountId = driver.PaymentAccountId, OrderId = order.ExternalId },
                        PayloadOut = new StatusContract { Status = true, DateCreated = DateTimeOffset.UtcNow },
                        RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
                    };
                
                    await new DriverPaymentCreationMessagePublisher(serviceProvider).PublishAsync(
                        driverPaymentCreationMessageContract);
                }
                
            }

            await databaseContext.SaveChangesAsync();

            return new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };
            
        }
    }
}