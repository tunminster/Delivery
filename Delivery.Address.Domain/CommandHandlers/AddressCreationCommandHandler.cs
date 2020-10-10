using System.Threading.Tasks;
using Delivery.Address.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Address.Domain.CommandHandlers
{
    public class AddressCreationCommandHandler : ICommandHandler<AddressCreationCommand, AddressCreationStatusContract>
    {
        private readonly ApplicationDbContext applicationDbContext;
        public AddressCreationCommandHandler (ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }

        public async Task<AddressCreationStatusContract> Handle(AddressCreationCommand command)
        {

            var address = new Database.Entities.Address()
            {
                CustomerId = command.AddressContract.CustomerId,
                AddressLine = command.AddressContract.AddressLine,
                Description = command.AddressContract.Description,
                City = command.AddressContract.City,
                PostCode = command.AddressContract.PostCode,
                Lat = command.AddressContract.Lat,
                Lng = command.AddressContract.Lng,
                Country = command.AddressContract.Country,
                Disabled = command.AddressContract.Disabled

            };

            applicationDbContext.Add(address);
            await applicationDbContext.SaveChangesAsync();

            return new AddressCreationStatusContract(true);

        }
    }
}