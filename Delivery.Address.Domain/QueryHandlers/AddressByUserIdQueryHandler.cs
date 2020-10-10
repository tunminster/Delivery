using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Address.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Address.Domain.QueryHandlers
{
    public class AddressByUserIdQueryHandler : IQueryHandler<AddressByUserIdQuery, List<AddressContract>>
    {
        private readonly ApplicationDbContext appDbContext;
        private readonly IMapper mapper;
        
        public AddressByUserIdQueryHandler(
            ApplicationDbContext appDbContext,
            IMapper mapper)
        {
            this.appDbContext = appDbContext;
            this.mapper = mapper;
        }
        
        public async Task<List<AddressContract>> Handle(AddressByUserIdQuery query)
        {
            var addressList = await appDbContext.Addresses.Where(x => x.CustomerId == query.UserId).ToListAsync();
            var addressContracts = mapper.Map<List<AddressContract>>(addressList);

            return addressContracts;
        }
    }
}