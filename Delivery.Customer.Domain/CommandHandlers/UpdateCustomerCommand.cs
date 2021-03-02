using Delivery.Customer.Domain.Contracts.RestContracts;

namespace Delivery.Customer.Domain.CommandHandlers
{
    public class UpdateCustomerCommand
    {
        public UpdateCustomerCommand(CustomerUpdateContract customerUpdateContract)
        {
            CustomerUpdateContract = customerUpdateContract;
        }
        public CustomerUpdateContract CustomerUpdateContract { get; }
    }
}