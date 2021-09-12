using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.CommandHandlers;
using Delivery.Order.Domain.Contracts.RestContracts.ApplicationFees;
using Delivery.Order.Domain.Factories;

namespace Delivery.Order.Domain.Handlers.CommandHandlers.Stripe.ApplicationFees
{
    public record ApplicationFeesCreationCommand(ApplicationFeesCreationContract ApplicationFeesCreationContract);
    
    public class ApplicationFeesCreationCommandHandler : ICommandHandler<ApplicationFeesCreationCommand, ApplicationFeesContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public ApplicationFeesCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<ApplicationFeesContract> Handle(ApplicationFeesCreationCommand command)
        {
            var platformFee = ApplicationFeeGenerator.GeneratorFees(command.ApplicationFeesCreationContract.SubTotal);
            
            // todo: refactor with calculation radius with store and customer address
            var random = new Random();

            var deliveryFee = ApplicationFeeGenerator.GenerateDeliveryFees(random.Next(1, 4));

            var totalAmount = command.ApplicationFeesCreationContract.SubTotal + platformFee + deliveryFee;
            var applicationFeesContract = new ApplicationFeesContract
            {
                PlatformFee = platformFee,
                DeliveryFee = deliveryFee,
                TotalAmount = totalAmount
            };

            return await Task.FromResult(applicationFeesContract);
        }
    }
}