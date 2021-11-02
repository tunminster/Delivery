using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverEarnings;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverOrder;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverEarnings
{
    public record DriverEarningDetailsQuery(DriverEarningDetailsQueryContract DriverEarningDetailsQueryContract) : IQuery<List<DriverEarningDetailsContract>>;

    public class DriverEarningDetailsQueryHandler : IQueryHandler<DriverEarningDetailsQuery, List<DriverEarningDetailsContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverEarningDetailsQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<DriverEarningDetailsContract>> Handle(DriverEarningDetailsQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var driverUser = executingRequestContextAdapter.GetAuthenticatedUser();
            
            var driver =
                await databaseContext.Drivers.SingleAsync(x => x.EmailAddress == driverUser.UserEmail);

            var driverEarningDetailList = await databaseContext.DriverOrders
                .Where(x => x.Status == DriverOrderStatus.Complete && x.DriverId == driver.Id
                                                                   && x.InsertionDateTime >=
                                                                   query.DriverEarningDetailsQueryContract.StartDate
                                                                   && x.InsertionDateTime <=
                                                                   query.DriverEarningDetailsQueryContract.EndDate)
                .Include(x => x.Order)
                .Select(x => new DriverEarningDetailsContract
                {
                    OrderId = x.Order.ExternalId,
                    OrderCreatedDate = x.Order.InsertionDateTime,
                    DeliveryFee = x.Order.DeliveryFees
                }).ToListAsync();

            return driverEarningDetailList;
        }
    }
}