using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Database.Enums;
using Delivery.Domain.CommandHandlers;
using Delivery.Domain.Contracts.V1.RestContracts.DistanceMatrix;
using Delivery.Domain.Services;
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
            
            var distanceMatrixContract = await new DistanceMatrixService(serviceProvider, executingRequestContextAdapter)
                .GetDistanceAsync(new DistanceMatrixRequestContract
                {
                    DestinationLatitude = command.ApplicationFeesCreationContract.CustomerLatitude,
                    DestinationLongitude = command.ApplicationFeesCreationContract.CustomerLongitude,
                    SourceLatitude = command.ApplicationFeesCreationContract.StoreLatitude,
                    SourceLongitude = command.ApplicationFeesCreationContract.StoreLongitude
                });

            var distance = distanceMatrixContract.Status == "OK"
                ? distanceMatrixContract.Rows.First()
                    .Elements.First().Distance.Value : 1000;
            
            var deliveryFee = ApplicationFeeGenerator.GenerateDeliveryFees(distance);

            var totalAmount = command.ApplicationFeesCreationContract.SubTotal + platformFee;

            if (command.ApplicationFeesCreationContract.OrderType == OrderType.DeliverTo)
            {
                totalAmount += deliveryFee;
            }
            
            var applicationFeesContract = new ApplicationFeesContract
            {
                PlatformFee = command.ApplicationFeesCreationContract.OrderType == OrderType.DeliverTo ? 0 :platformFee,
                DeliveryFee = deliveryFee,
                TotalAmount = totalAmount
            };

            return await Task.FromResult(applicationFeesContract);
        }
    }
}