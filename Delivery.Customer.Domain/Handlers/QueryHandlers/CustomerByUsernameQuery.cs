using Delivery.Customer.Domain.Contracts;
using Delivery.Domain.QueryHandlers;

namespace Delivery.Customer.Domain.Handlers.QueryHandlers
{
    public class CustomerByUsernameQuery : IQuery<CustomerContract>
    {
        public CustomerByUsernameQuery(string username)
        {
            Username = username;
        }
        public string Username { get; }
    }
}