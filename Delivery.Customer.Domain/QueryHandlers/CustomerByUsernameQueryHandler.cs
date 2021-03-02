using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Address.Domain.Contracts;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Customer.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Customer.Domain.QueryHandlers
{
    public class CustomerByUsernameQueryHandler : IQueryHandler<CustomerByUsernameQuery, CustomerContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public CustomerByUsernameQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<CustomerContract> Handle(CustomerByUsernameQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var customer = await databaseContext.Customers.Where(x => x.Username.ToLower() == query.Username.ToLower())
                .Include(x => x.Addresses).FirstOrDefaultAsync();

            var customerContract = new CustomerContract
            {
                Id = customer.Id,
                Username = customer.Username,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                ContactNumber = customer.ContactNumber
            };

            customerContract.Addresses = new List<AddressContract>();

            foreach (var address in customer.Addresses)
            {
                customerContract.Addresses.Add(new AddressContract
                {
                    Id = address.Id,
                    AddressLine = address.AddressLine,
                    City = address.City,
                    Country = address.Country,
                    CustomerId = address.CustomerId,
                    Description = address.Description,
                    Disabled = address.Disabled,
                    Lat = address.Lat,
                    Lng = address.Lng,
                    PostCode = address.PostCode
                });
            }

            return customerContract;
        }
    }
}