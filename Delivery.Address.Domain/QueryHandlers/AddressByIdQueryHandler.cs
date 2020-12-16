using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Address.Domain.Contracts;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Address.Domain.QueryHandlers
{
    public class AddressByIdQueryHandler : IQueryHandler<AddressByIdQuery, AddressContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public AddressByIdQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<AddressContract> Handle(AddressByIdQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var result = await databaseContext.Addresses.FirstOrDefaultAsync(x => x.Id == query.AddressId);

            var addressContract = new AddressContract
            {
                Id = result.Id,
                AddressLine = result.AddressLine,
                City = result.City,
                Country = result.Country,
                CustomerId = result.CustomerId,
                Description = result.Description,
                Disabled = result.Disabled,
                Lat = result.Lat,
                Lng = result.Lng,
                PostCode = result.PostCode
            };
            return addressContract;
        }
        
        
    }
}