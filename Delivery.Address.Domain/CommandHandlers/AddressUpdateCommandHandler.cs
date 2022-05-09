using System;
using System.Threading.Tasks;
using Delivery.Address.Domain.Contracts;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Address.Domain.CommandHandlers
{
    public class AddressUpdateCommandHandler : ICommandHandler<AddressUpdateCommand, AddressUpdateStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public AddressUpdateCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<AddressUpdateStatusContract> HandleAsync(AddressUpdateCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var address = await databaseContext.Addresses.FindAsync(command.AddressContract.Id);

            var addressUpdateStatusContract = new AddressUpdateStatusContract();
            if (address == null)
            {
                return addressUpdateStatusContract;
            }

            address.City = command.AddressContract.City;
            address.Country = command.AddressContract.Country;
            address.Description = command.AddressContract.Description;
            address.Lat = command.AddressContract.Lat;
            address.Lng = command.AddressContract.Lng;
            address.AddressLine = command.AddressContract.AddressLine;
            address.PostCode = command.AddressContract.PostCode;
            
            databaseContext.Entry(address).State = EntityState.Modified;
            await databaseContext.SaveChangesAsync();
            addressUpdateStatusContract.AddressUpdated = true;

            return addressUpdateStatusContract;
        }
    }
}