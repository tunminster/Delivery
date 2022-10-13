using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverIndex;
using Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverPayments;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverIndex;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverPayments;
using Delivery.Driver.Domain.Handlers.CommandHandlers.DriverIndex;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverAssignment;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverIndex;
using Delivery.Driver.Domain.Handlers.MessageHandlers.DriverPayments;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.CommandHandlers.DriverAssignment
{
    public record DriverOrderActionCommand(DriverOrderActionContract DriverOrderActionContract);
    
    public class DriverOrderActionCommandHandler : ICommandHandler<DriverOrderActionCommand, StatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverOrderActionCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StatusContract> HandleAsync(DriverOrderActionCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var driver = await databaseContext.Drivers.SingleAsync(x =>
                x.EmailAddress == executingRequestContextAdapter.GetAuthenticatedUser().UserEmail);

            var driverId = driver.ExternalId;
            var order = await databaseContext.Orders.SingleOrDefaultAsync(x =>
                x.ExternalId == command.DriverOrderActionContract.OrderId) ?? throw new InvalidOperationException($"Expected order by order id: {command.DriverOrderActionContract.OrderId}.");

            var driverOrder =
                await databaseContext.DriverOrders.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x =>
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

            if (command.DriverOrderActionContract.DriverOrderStatus == DriverOrderStatus.Rejected)
            {
                driver.IsOrderAssigned = false;
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

                driver.IsOrderAssigned = false;
                
                await new DriverOrderCompleteMessagePublisher(serviceProvider).PublishAsync(
                    driverOrderCompleteMessageContract);
                
                // indexing complete delivery
                // await new DriverOrderIndexCommandHandler(serviceProvider, executingRequestContextAdapter)
                //     .Handle(new DriverOrderIndexCommand(new DriverOrderIndexCreationContract
                //     {
                //         DriverId = driver.ExternalId,
                //         OrderId = order.ExternalId
                //     }));
                
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
            
            var statusContract = new StatusContract
            {
                Status = true,
                DateCreated = DateTimeOffset.UtcNow
            };

            if (command.DriverOrderActionContract.DriverOrderStatus == DriverOrderStatus.Complete || command.DriverOrderActionContract.DriverOrderStatus == DriverOrderStatus.Rejected)
            {
                // indexing driver
                
                var driverIndexMessageContract = new DriverIndexMessageContract
                {
                    PayloadIn = new DriverIndexCreationContract {DriverId = driverId},
                    PayloadOut = statusContract,
                    RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
                };
            
                await new DriverIndexMessagePublisher(serviceProvider).PublishAsync(driverIndexMessageContract);
                
            }
            
            
            return statusContract;
        }
    }
}