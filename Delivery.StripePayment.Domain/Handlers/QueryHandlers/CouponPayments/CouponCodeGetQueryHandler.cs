using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.QueryHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;
using Delivery.StripePayment.Domain.Converters.CouponPayments;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.StripePayment.Domain.Handlers.QueryHandlers.CouponPayments
{
    public record CouponCodeGetQuery(string CouponCode) : IQuery<CouponCodeContract>;
    
    public class CouponCodeGetQueryHandler : IQueryHandler<CouponCodeGetQuery, CouponCodeContract>
    {
        private IServiceProvider serviceProvider;
        private IExecutingRequestContextAdapter executingRequestContextAdapter;
        public CouponCodeGetQueryHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<CouponCodeContract> Handle(CouponCodeGetQuery query)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            
            // retrieve promotion code
            var options = new PromotionCodeListOptions
            {
                Limit = 1,
                Code = query.CouponCode
            };

            var service = new PromotionCodeService();
            var promotionCodes = await service.ListAsync(options);

            if (!promotionCodes.Any())
            {
                return null;
            }
            
            var promoCode = promotionCodes.First();
            var couponCodeContract = promoCode.ConvertToCouponCodeContract();
            
            return couponCodeContract;
        }
    }
}