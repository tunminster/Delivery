using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverProfile;
using Delivery.Driver.Domain.Converters;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverProfile
{
    public record DriverProfileQuery : IQuery<DriverProfileContract>
    {
        public string EmailAddress { get; set; } = string.Empty;
    }
    
    public class DriverProfileQueryHandler : IQueryHandler<DriverProfileQuery, DriverProfileContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public DriverProfileQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<DriverProfileContract> Handle(DriverProfileQuery query)
        {
            // todo: query from elastic search later
            
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var driver = await databaseContext.Drivers.FirstOrDefaultAsync(x => x.EmailAddress == query.EmailAddress);

            return driver.ConvertToDriverProfile();
        }
    }
}