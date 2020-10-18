using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Customer.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Customer.Domain.QueryHandlers
{
    public class CustomerByUsernameQueryHandler : IQueryHandler<CustomerByUsernameQuery, CustomerContract>
    {
        private readonly ApplicationDbContext appDbContext;
        private readonly IMapper mapper;
        
        public CustomerByUsernameQueryHandler(ApplicationDbContext appDbContext, IMapper mapper)
        {
            this.appDbContext = appDbContext;
            this.mapper = mapper;
        }
        
        public async Task<CustomerContract> Handle(CustomerByUsernameQuery query)
        {
            var customer = await appDbContext.Customers.Where(x => x.Username.ToLower() == query.Username.ToLower())
                .Include(x => x.Addresses).FirstOrDefaultAsync();

            return mapper.Map <CustomerContract>(customer);
        }
    }
}