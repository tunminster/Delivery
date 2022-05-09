using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Customer.Domain.Contracts.V1.RestContracts;
using Delivery.Customer.Domain.Handlers.CommandHandlers;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Customer.Domain.Handlers.CommandHandlers
{
    public class UpdateCustomerCommandHandler: ICommandHandler<UpdateCustomerCommand, CustomerUpdateStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public UpdateCustomerCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<CustomerUpdateStatusContract> HandleAsync(UpdateCustomerCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var customer =
                await databaseContext.Customers.FirstOrDefaultAsync(x => x.Id == command.CustomerUpdateContract.CustomerId);

            customer.FirstName = command.CustomerUpdateContract.FirstName;
            customer.LastName = command.CustomerUpdateContract.LastName;
            customer.ContactNumber = command.CustomerUpdateContract.ContactNumber;
            
            await databaseContext.SaveChangesAsync();

            var customerUpdateStatusContract = new CustomerUpdateStatusContract
            {
                CustomerId = customer.Id,
                Message = "customer updated",
                UpdateDate = DateTimeOffset.UtcNow
            };

            return customerUpdateStatusContract;
        }
    }
}