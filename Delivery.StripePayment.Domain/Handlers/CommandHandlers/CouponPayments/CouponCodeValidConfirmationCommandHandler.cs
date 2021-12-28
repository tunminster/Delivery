using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.StripePayment.Domain.Handlers.CommandHandlers.CouponPayments
{
    public record CouponCodeValidConfirmationCommand(CouponCodeConfirmationQueryContract CouponCodeConfirmationQueryContract);
    public class CouponCodeValidConfirmationCommandHandler : ICommandHandler<CouponCodeValidConfirmationCommand, CouponCodeStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public CouponCodeValidConfirmationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<CouponCodeStatusContract> Handle(CouponCodeValidConfirmationCommand command)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            
            // retrieve promotion code
            var options = new PromotionCodeListOptions
            {
                Limit = 1,
                Code = command.CouponCodeConfirmationQueryContract.CouponCode
            };

            var service = new PromotionCodeService();
            var promotionCodes = await service.ListAsync(options);

            if (!promotionCodes.Any())
                return new CouponCodeStatusContract
                {
                    PromoCode = command.CouponCodeConfirmationQueryContract.CouponCode,
                    Status = false,
                    Message = string.Empty
                };
            
            var promoCode = promotionCodes.First();
            
            if (!promoCode.Active || !promoCode.Coupon.Valid)
                return new CouponCodeStatusContract
                {
                    PromoCode = command.CouponCodeConfirmationQueryContract.CouponCode,
                    Status = false,
                    Message = string.Empty
                };
            
            var couponCodeConfirmationQueryStatusContract = new CouponCodeStatusContract
            {
                PromoCode = promoCode.Code,
                Status = promoCode.Active,
                Message = string.Empty
            };

            return couponCodeConfirmationQueryStatusContract;
        }
    }
}