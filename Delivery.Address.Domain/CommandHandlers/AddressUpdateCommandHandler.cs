using System.Threading.Tasks;
using Delivery.Address.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Address.Domain.CommandHandlers
{
    public class AddressUpdateCommandHandler : ICommandHandler<AddressUpdateCommand, AddressUpdateStatusContract>
    {
        private readonly ApplicationDbContext applicationDbContext;
        
        public AddressUpdateCommandHandler (ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }
        
        public async Task<AddressUpdateStatusContract> Handle(AddressUpdateCommand command)
        {
            var address = await applicationDbContext.Addresses.FindAsync(command.AddressContract.Id);

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
            
            applicationDbContext.Entry(address).State = EntityState.Modified;
            await applicationDbContext.SaveChangesAsync();
            addressUpdateStatusContract.AddressUpdated = true;

            return addressUpdateStatusContract;
        }
    }
}