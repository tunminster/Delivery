using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.DataAccess;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.QueryHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;
using Delivery.StripePayment.Domain.Converters.CouponPayments;
using Microsoft.EntityFrameworkCore;

namespace Delivery.StripePayment.Domain.Handlers.QueryHandlers.CouponPayments
{
    public record CouponCodeGetDiscountAmountQuery(string CouponCode) : IQuery<CouponCodeContract>;
    
    public class CouponCodeGetDiscountAmountQueryHandler : IQueryHandler<CouponCodeGetDiscountAmountQuery, CouponCodeContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public CouponCodeGetDiscountAmountQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<CouponCodeContract> Handle(CouponCodeGetDiscountAmountQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.DriverOrder>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            var driverCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{nameof(CouponCodeGetDiscountAmountQuery).ToLowerInvariant()}-{query.CouponCode}";
            
            var couponCodeContract = await dataAccess.GetCachedItemsAsync(
                driverCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.CouponCodes
                    .Where(x => x.PromotionCode == query.CouponCode)
                    .Select(x => x.ConvertToCouponCodeContract()).SingleAsync());

            return couponCodeContract;
        }
    }
}