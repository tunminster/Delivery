using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Address.Domain.Contracts;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Address.Domain.QueryHandlers
{
    public class AddressByIdQueryHandler : IQueryHandler<AddressByIdQuery, AddressContract>
    {
        private readonly ApplicationDbContext appDbContext;
        private readonly IMapper mapper;
        
        public AddressByIdQueryHandler(
            ApplicationDbContext appDbContext,
            IMapper mapper)
        {
            this.appDbContext = appDbContext;
            this.mapper = mapper;
        }
        
        public async Task<AddressContract> Handle(AddressByIdQuery query)
        {
            var result = await appDbContext.Addresses.FirstOrDefaultAsync(x => x.Id == query.AddressId);
            var addressContract = mapper.Map<AddressContract>(result);
            return addressContract;
        }
    }
}