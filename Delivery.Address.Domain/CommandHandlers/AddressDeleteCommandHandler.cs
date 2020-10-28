using System.Threading.Tasks;
using Delivery.Address.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Address.Domain.CommandHandlers
{
    public class AddressDeleteCommandHandler : ICommandHandler<AddressDeleteCommand, AddressDeleteStatusContract>
    {
        private readonly ApplicationDbContext applicationDbContext;
        
        public AddressDeleteCommandHandler (ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }
        
        public async Task<AddressDeleteStatusContract> Handle(AddressDeleteCommand command)
        {
            
            var address = await applicationDbContext.Addresses.FindAsync(command.AddressId);
            var addressDeleteStatusContract = new AddressDeleteStatusContract();
            
            if (address == null)
            {
                return addressDeleteStatusContract;
            }
            
            applicationDbContext.Addresses.Remove(address);
            await applicationDbContext.SaveChangesAsync();
            addressDeleteStatusContract.AddressDeleted = true;

            return addressDeleteStatusContract;
        }
    }
}