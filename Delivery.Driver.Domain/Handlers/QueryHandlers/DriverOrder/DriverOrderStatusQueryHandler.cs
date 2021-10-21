using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;
using Delivery.Driver.Domain.Converters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverOrder
{
    public record DriverOrderStatusQuery : IQuery<List<DriverOrderDetailsContract>>
    {
        public DriverOrderStatusRequestContract DriverOrderStatusRequestContract { get; init; } = new();
    }
    
    public class DriverOrderStatusQueryHandler : IQueryHandler<DriverOrderStatusQuery, List<DriverOrderDetailsContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverOrderStatusQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<DriverOrderDetailsContract>> Handle(DriverOrderStatusQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail ?? throw new InvalidOperationException("Expected an authenticated user.");

            var driverOrder = await databaseContext.DriverOrders
                .Include(x => x.Driver)
                .Include(x => x.Order)
                .ThenInclude(x => x.Store)
                .Include(x => x.Order.OrderItems)
                .ThenInclude(x => x.Product)
                .Include(x => x.Order.Address)
                .Where(x => x.Driver.EmailAddress == userEmail &&
                            x.Status == query.DriverOrderStatusRequestContract.DriverOrderStatus &&
                            x.InsertionDateTime >= query.DriverOrderStatusRequestContract.FromDate)
                .ToListAsync();
            
            return driverOrder.ConvertToDriverOrderDetailsContract();
        }
    }
}