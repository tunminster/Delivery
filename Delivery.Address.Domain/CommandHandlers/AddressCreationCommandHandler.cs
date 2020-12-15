using System;
using System.Threading.Tasks;
using Delivery.Address.Domain.Contracts;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Address.Domain.CommandHandlers
{
    public class AddressCreationCommandHandler : ICommandHandler<AddressCreationCommand, AddressCreationStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public AddressCreationCommandHandler ( IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<AddressCreationStatusContract> Handle(AddressCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var address = new Database.Entities.Address
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

            databaseContext.Add(address);
            await databaseContext.SaveChangesAsync();

            return new AddressCreationStatusContract(true);

        }
    }
}