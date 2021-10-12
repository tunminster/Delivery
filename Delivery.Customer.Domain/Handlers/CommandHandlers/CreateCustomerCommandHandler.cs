using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Customer.Domain.Handlers.CommandHandlers
{
    public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, bool>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public CreateCustomerCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<bool> Handle(CreateCustomerCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            await databaseContext.Customers.AddAsync(new Database.Entities.Customer { IdentityId = command.CustomerCreationContract.IdentityId, Username = command.CustomerCreationContract.Username, FirstName = command.CustomerCreationContract.FirstName, LastName = command.CustomerCreationContract.LastName, ContactNumber =  command.CustomerCreationContract.ContactNumber});
            await databaseContext.SaveChangesAsync();

            return true;
        }
    }
}