using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Customer.Domain.Handlers.CommandHandlers
{
    public record DeleteCustomerCommand(string Username);
    
    public class DeleteCustomerCommandHandler : ICommandHandler<DeleteCustomerCommand, bool>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DeleteCustomerCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<bool> HandleAsync(DeleteCustomerCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var customer = await databaseContext.Customers.FirstOrDefaultAsync(x => x.Username == command.Username);
            if (customer is not null)
            {
                customer.IsDeleted = true;
            }

            return await databaseContext.SaveChangesAsync() > 0;
        }
    }
}