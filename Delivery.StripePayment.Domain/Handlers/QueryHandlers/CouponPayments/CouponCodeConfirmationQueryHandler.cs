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
    public record CouponCodeConfirmationQuery(CouponCodeConfirmationQueryContract CouponCodeConfirmationQueryContract) 
        : IQuery<CouponCodeStatusContract>;
    
    public class CouponCodeConfirmationQueryHandler : IQueryHandler<CouponCodeConfirmationQuery, CouponCodeStatusContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public CouponCodeConfirmationQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<CouponCodeStatusContract> Handle(CouponCodeConfirmationQuery query)
        {
            await using var dataAccess = new ShardedDataAccess<PlatformDbContext, Database.Entities.DriverOrder>(
                serviceProvider, () => PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter));
            
            var databaseContext = await dataAccess.ReusableDatabaseContext.GetOrCreateContextAsync();
            var driverCacheKey = $"Database-{executingRequestContextAdapter.GetShard().Key}-{nameof(CouponCodeConfirmationQuery).ToLowerInvariant()}-{query.CouponCodeConfirmationQueryContract.CouponCode}";
            
            var couponCodeContract = await dataAccess.GetCachedItemsAsync(
                driverCacheKey,
                databaseContext.GlobalDatabaseCacheRegion,
                async () => await databaseContext.CouponCodes
                    .Where(x => x.PromotionCode == query.CouponCodeConfirmationQueryContract.CouponCode)
                    .Select(x => x.ConvertToCouponCodeContract()).SingleAsync());

            if (couponCodeContract != null && couponCodeContract.RedeemBy > DateTimeOffset.UtcNow)
            {
                var couponCodeConfirmationQueryStatusContract = new CouponCodeStatusContract
                {
                    Status = true,
                    PromoCode = couponCodeContract.PromoCode,
                    Message = string.Empty
                };

                return couponCodeConfirmationQueryStatusContract;
            }

            return new CouponCodeStatusContract
            {
                Status = false,
                PromoCode = string.Empty,
                Message = $"{query.CouponCodeConfirmationQueryContract.CouponCode} is expired or not valid."
            };
        }
    }
}