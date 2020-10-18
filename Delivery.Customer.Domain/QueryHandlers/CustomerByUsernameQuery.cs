using System.Collections.Generic;
using Delivery.Customer.Domain.Contracts;
using Delivery.Domain.QueryHandlers;

namespace Delivery.Customer.Domain.QueryHandlers
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