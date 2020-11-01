using System.Threading.Tasks;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;

namespace Delivery.Customer.Domain.CommandHandlers
{
    public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, bool>
    {
        private readonly ApplicationDbContext applicationDbContext;

        public CreateCustomerCommandHandler(ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }
        
        public async Task<bool> Handle(CreateCustomerCommand command)
        {
            await applicationDbContext.Customers.AddAsync(new Database.Entities.Customer { IdentityId = command.CustomerCreationContract.IdentityId, Username = command.CustomerCreationContract.Username });
            await applicationDbContext.SaveChangesAsync();

            return true;
        }
    }
}