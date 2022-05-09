using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments;
using Delivery.StripePayment.Domain.Converters.StripePayments;
using Microsoft.EntityFrameworkCore;

namespace Delivery.StripePayment.Domain.Handlers.CommandHandlers.StripePaymentCreation
{
    public class StripePaymentCreationCommandHandler : ICommandHandler<StripePaymentCreationCommand, StripePaymentCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StripePaymentCreationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StripePaymentCreationStatusContract> HandleAsync(StripePaymentCreationCommand command)
        {
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);

            var order = await databaseContext.Orders.FirstOrDefaultAsync(x =>
                x.ExternalId == command.StripePaymentCreationContract.OrderId);

            if (order == null)
            {
                throw new InvalidOperationException($"{command.StripePaymentCreationContract.OrderId} is not existed.")
                    .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
            }
            
            var stripePayment = StripePaymentConverter.Convert(command.StripePaymentCreationContract, order.Id);
            stripePayment.InsertedBy = executingRequestContextAdapter.GetAuthenticatedUser().UserEmail;
            stripePayment.InsertionDateTime = DateTimeOffset.UtcNow;
            stripePayment.ExternalId = executingRequestContextAdapter.GetShard().GenerateExternalId();

            await databaseContext.StripePayments.AddAsync(stripePayment);
            await databaseContext.SaveChangesAsync();

            var stripePaymentCreationStatusContract = new StripePaymentCreationStatusContract
            {
                StripePaymentId = stripePayment.ExternalId,
                DateCreated = DateTimeOffset.UtcNow
            };
            return stripePaymentCreationStatusContract;
        }
    }
}