using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.Helpers;
using Delivery.Domain.QueryHandlers;
using Delivery.Shop.Domain.Contracts.V1.RestContracts.ShopPaymentHistory;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Shop.Domain.Handlers.QueryHandlers.ShopPaymentHistory
{
    public record ShopPaymentHistoryQuery(ShopPaymentHistoryQueryContract ShopPaymentHistoryQueryContract) : IQuery<List<ShopPaymentHistoryContract>>;
    
    public class ShopPaymentHistoryQueryHandler : IQueryHandler<ShopPaymentHistoryQuery, List<ShopPaymentHistoryContract>>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;
        
        public ShopPaymentHistoryQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<List<ShopPaymentHistoryContract>> Handle(ShopPaymentHistoryQuery query)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var userEmail = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail;

            var storeUser = await databaseContext.StoreUsers.SingleOrDefaultAsync(x => x.Username == userEmail) ?? throw new InvalidOperationException($"Expected store user.");
            
            var firstMondayOfYear = GetFirstMondayOfYear(query.ShopPaymentHistoryQueryContract.Year);
            var paymentHistory = databaseContext.Orders.Where(x => x.StoreId == storeUser.StoreId
                                                                   && x.InsertionDateTime.Month ==
                                                                   query.ShopPaymentHistoryQueryContract.Month
                                                                   && x.InsertionDateTime.Year ==
                                                                   query.ShopPaymentHistoryQueryContract.Year)
                .ToList()
                .GroupBy(x => (int)(x.InsertionDateTime - firstMondayOfYear).TotalDays / 7)
                .Select(sl => new ShopPaymentHistoryContract
                {
                    TotalAmount = sl.Sum(s => s.SubTotal) + sl.Sum(s => s.TaxFees),
                    TotalOrders = sl.Count(),
                    DateRange =
                        $"{DateTimeHelper.FirstDateOfWeek(query.ShopPaymentHistoryQueryContract.Year, int.Parse(sl.Key.ToString()) + 1):dd MMMM} {DateTimeHelper.FirstDateOfWeek(query.ShopPaymentHistoryQueryContract.Year, int.Parse(sl.Key.ToString()) + 1).AddDays(6):dd MMMM}"
                });


            return paymentHistory.ToList();
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