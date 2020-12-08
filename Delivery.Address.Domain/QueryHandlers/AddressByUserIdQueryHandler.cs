using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Delivery.Address.Domain.Contracts;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Address.Domain.QueryHandlers
{
    public class AddressByUserIdQueryHandler : IQueryHandler<AddressByUserIdQuery, List<AddressContract>>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public AddressByUserIdQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<AddressContract>> Handle(AddressByUserIdQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var addressList = await databaseContext.Addresses.Where(x => x.CustomerId == query.UserId).Select(x => new AddressContract
            {
                Id = x.Id,
                AddressLine = x.AddressLine,
                City = x.City,
                Country = x.Country,
                CustomerId = x.CustomerId,
                Description = x.Description,
                Disabled = x.Disabled,
                Lat = x.Lat,
                Lng = x.Lng,
                PostCode = x.PostCode
            }).ToListAsync();

            return addressList;
        }
    }
}