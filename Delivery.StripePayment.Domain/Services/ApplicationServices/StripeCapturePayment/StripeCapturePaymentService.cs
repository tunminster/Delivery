using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.StripePayment.Domain.CommandHandlers.PaymentIntent.PaymentIntentConfirmation;
using Delivery.StripePayment.Domain.Contracts.V1.MessageContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments;
using Delivery.StripePayment.Domain.Handlers.MessageHandlers;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.StripePayment.Domain.Services.ApplicationServices.StripeCapturePayment
{
    public class StripeCapturePaymentService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public StripeCapturePaymentService(IServiceProvider serviceProvider,
            IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }

        public async Task<StripeCapturePaymentServiceResult> ExecuteStripeCapturePaymentCreationWorkflowAsync(
            StripeCapturePaymentServiceRequest request)
        {
            var paymentIntentConfirmationCommand =
                new PaymentIntentConfirmationCommand(request.StripePaymentCaptureCreationContract);

            var stripePaymentCaptureCreationStatus =
                await new PaymentIntentConfirmationCommandHandler(serviceProvider, executingRequestContextAdapter)
                    .Handle(paymentIntentConfirmationCommand);
            
            // publish stripe payment message
            var stripePaymentCreationContract = new StripePaymentCreationContract
            {
                OrderId = stripePaymentCaptureCreationStatus.OrderId,
                StripePaymentIntentId = request.StripePaymentCaptureCreationContract.StripePaymentIntentId,
                StripePaymentMethodId = request.StripePaymentCaptureCreationContract.StripePaymentMethodId,
                PaymentStatus = stripePaymentCaptureCreationStatus.PaymentStatus,
                Captured = stripePaymentCaptureCreationStatus.Captured,
                AmountCaptured = stripePaymentCaptureCreationStatus.AmountCaptured,
                FailureCode = stripePaymentCaptureCreationStatus.FailureCode,
                FailureMessage = stripePaymentCaptureCreationStatus.FailureMessage,
                ReceiptUrl = stripePaymentCaptureCreationStatus.ReceiptUrl
            };

            var stripePaymentCreationStatusContract = new StripePaymentCreationStatusContract
            {
                StripePaymentId = executingRequestContextAdapter.GetShard().GenerateExternalId(),
                DateCreated = DateTimeOffset.UtcNow
            };

            await PublishStripePaymentCreationMessageAsync(stripePaymentCreationContract,
                stripePaymentCreationStatusContract);

            var stripeCapturePaymentServiceResult =
                new StripeCapturePaymentServiceResult(stripePaymentCaptureCreationStatus);

            return stripeCapturePaymentServiceResult;

        }
        
        private async Task PublishStripePaymentCreationMessageAsync(StripePaymentCreationContract stripePaymentCreationContract,
            StripePaymentCreationStatusContract stripePaymentCreationStatusContract)
        {
            var paymentCreationMessageContract = new PaymentCreationMessageContract
            {
                PayloadIn = stripePaymentCreationContract,
                PayloadOut = stripePaymentCreationStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };

            await new PaymentCreationMessagePublisher(serviceProvider).PublishAsync(paymentCreationMessageContract);
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(PublishStripePaymentCreationMessageAsync)} published payment creation message", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
        }
    }
}