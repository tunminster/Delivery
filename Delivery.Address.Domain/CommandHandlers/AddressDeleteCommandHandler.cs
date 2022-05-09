using System;
using System.Threading.Tasks;
using Delivery.Address.Domain.Contracts;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Microsoft.Graph;

namespace Delivery.Address.Domain.CommandHandlers
{
    public class AddressDeleteCommandHandler : ICommandHandler<AddressDeleteCommand, AddressDeleteStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public AddressDeleteCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<AddressDeleteStatusContract> HandleAsync(AddressDeleteCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var address = await databaseContext.Addresses.FindAsync(command.AddressId);
            var addressDeleteStatusContract = new AddressDeleteStatusContract();
            
            if (address == null)
            {
                return addressDeleteStatusContract;
            }
            
            databaseContext.Addresses.Remove(address);
            await databaseContext.SaveChangesAsync();
            addressDeleteStatusContract.AddressDeleted = true;

            return addressDeleteStatusContract;
        }
    }
}