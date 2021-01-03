using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;

namespace Delivery.StripePayment.Domain.CommandHandlers.StripePayments
{
    public class StripePaymentCreationCommandHandler : ICommandHandler<StripePaymentCreationCommand, StripePaymentCaptureCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StripePaymentCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        
        public async Task<StripePaymentCaptureCreationStatusContract> Handle(StripePaymentCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            
            throw new System.NotImplementedException();
        }
    }
}