using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.Constants;
using Delivery.Domain.Helpers;
using Delivery.Domain.QueryHandlers;
using Delivery.Driver.Domain.Contracts.V1.Enums.DriverEarnings;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverAssignment;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverEarnings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Delivery.Driver.Domain.Handlers.QueryHandlers.DriverEarnings
{
    public record DriverEarningsQuery(DriverEarningQueryContract DriverEarningQueryContract, int Page, int PageSize = 52) : IQuery<List<DriverEarningContract>>;
    
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
            
            var firstMondayOfYear = GetFirstMondayOfYear(query.DriverEarningQueryContract.Year);

            if (query.DriverEarningQueryContract.DriverEarningFilter == DriverEarningFilter.Monthly)
            {
                var earningMonthly = databaseContext.DriverOrders
                    .Where(x => x.Status == DriverOrderStatus.Complete && x.DriverId == driver.Id
                                                                       && x.InsertionDateTime.Year ==
                                                                       query.DriverEarningQueryContract.Year)
                    .Include(x => x.Order)
                    .ToList()
                    .GroupBy(x => new { Month = x.InsertionDateTime.Month })
                    .Select(sl => new DriverEarningContract
                    {
                        TotalOrders = sl.Count(),
                        TotalAmount = sl.Sum(s => s.Order.DeliveryFees),
                        DateRange = $"{sl.Key.Month} {query.DriverEarningQueryContract.Year}"
                    });
                return earningMonthly.ToList();
            }
            
            var earnings = databaseContext.DriverOrders
                .Where(x => x.Status == DriverOrderStatus.Complete && x.DriverId == driver.Id 
                                                                   && x.InsertionDateTime.Year == query.DriverEarningQueryContract.Year
                                                                   && x.InsertionDateTime.Month == query.DriverEarningQueryContract.Month)
                .Include(x => x.Order)
                .ToList()
                .GroupBy(x => (int)(x.InsertionDateTime - firstMondayOfYear).TotalDays / 7)
                .Select(sl => new DriverEarningContract
                {
                    TotalOrders = sl.Count(),
                    TotalAmount = sl.Sum(s => s.Order.DeliveryFees),
                    DateRange = $"{DateTimeHelper.FirstDateOfWeek(query.DriverEarningQueryContract.Year, int.Parse(sl.Key.ToString()) + 1):dd MMMM} {DateTimeHelper.FirstDateOfWeek(query.DriverEarningQueryContract.Year, int.Parse(sl.Key.ToString()) + 1).AddDays(6):dd MMMM}"
                });
            

            return earnings.ToList();
        }

        private async Task<List<DriverEarningContract>> GetDriverOrderAsync(DriverEarningsQuery query, string driverId)
        {
            // todo: aggreation is not working
            var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();
            var driverOrderSearchResult = await elasticClient.SearchAsync<DriverOrderContract>(x =>
                x.Index(
                        $"{ElasticSearchIndexConstants.DriverOrdersIndex}{executingRequestContextAdapter.GetShard().Key.ToLower()}")
                    .Query(q =>
                        q.Bool(bl =>
                            bl.Filter(fl =>
                                fl.Terms(tm =>
                                    tm.Field(fd => fd.DriverId).Terms(driverId))))
                        && q.Bool(b =>
                            b.Filter(f =>
                                f.Terms(ts =>
                                    ts.Field(tsf => tsf.Status).Terms(DriverOrderStatus.Complete))))
                    )
                    .Aggregations(a =>
                        a.DateHistogram("DateRange", d =>
                            d.Field(df => df.DateCreated)
                                .CalendarInterval(DateInterval.Week)
                                .Aggregations(ag =>
                                    ag.Sum("TotalAmount", ags =>
                                        ags.Field(af => af.DeliveryFee)))
                                .Aggregations(ag => 
                                    ag.ValueCount("TotalOrders", av =>  
                                        av.Field(af => af)))))
                    .Source()
                    .From((query.Page - 1) * query.PageSize)
                    .Size(query.PageSize)
            );
            
            var result = driverOrderSearchResult.Aggregations;
            // var driverContracts = driverOrderSearchResult.Documents
            //     .Zip(driverOrderSearchResult.Fields, (s, d) => new DriverEarningContract
            //     {
            //         DateRange = d.
            //     }).ToList();

            return null;
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