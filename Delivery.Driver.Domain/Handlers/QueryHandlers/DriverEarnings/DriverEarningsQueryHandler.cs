using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverEarnings;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverEarnings
{
    public record DriverEarningsQuery(DriverEarningQueryContract DriverEarningQueryContract) : IQuery<List<DriverEarningContract>>;
    
    public class DriverEarningsQueryHandler : IQueryHandler<DriverEarningsQuery, List<DriverEarningContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public DriverEarningsQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<DriverEarningContract>> Handle(DriverEarningsQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var driverUser = executingRequestContextAdapter.GetAuthenticatedUser();

            var driver =
                await databaseContext.Drivers.SingleAsync(x => x.EmailAddress == driverUser.UserEmail);
            
            var firstMondayOfYear = this.GetFirstMondayOfYear(DateTimeOffset.Now.Year);
            var earnings = databaseContext.DriverOrders.Where(x => x.Status == DriverOrderStatus.Complete && x.DriverId == driver.Id)
                .Include(x => x.Order)
                .AsEnumerable()
                .GroupBy(x => (int)(x.InsertionDateTime - firstMondayOfYear).TotalDays / 7)
                .Select(sl => new DriverEarningContract
                {
                    TotalOrders = sl.Count(),
                    TotalAmount = sl.Sum(s => s.Order.DeliveryFees),
                    DateRange = sl.Key.ToString()
                });

            return earnings.ToList();
        }
        
        private DateTimeOffset GetFirstMondayOfYear(int year)
        {
            var dt = new DateTimeOffset(year, 1, 1, 1, 1, 1,new TimeSpan(1, 0, 0));
            while (dt.DayOfWeek != DayOfWeek.Monday)
            {
                dt = dt.AddDays(1);
            }

            return dt;
        }
    }
}