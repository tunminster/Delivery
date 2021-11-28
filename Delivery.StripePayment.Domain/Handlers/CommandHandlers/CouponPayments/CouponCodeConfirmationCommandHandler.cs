using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;

namespace Delivery.StripePayment.Domain.Handlers.CommandHandlers.CouponPayments
{
    public record CouponCodeConfirmationCommand(CouponCodeConfirmationQueryContract CouponCodeConfirmationQueryContract);
    public class CouponCodeConfirmationCommandHandler : ICommandHandler<CouponCodeConfirmationCommand, CouponCodeConfirmationQueryStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public CouponCodeConfirmationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public Task<CouponCodeConfirmationQueryStatusContract> Handle(CouponCodeConfirmationCommand command)
        {
            
            throw new System.NotImplementedException();
        }
    }
}