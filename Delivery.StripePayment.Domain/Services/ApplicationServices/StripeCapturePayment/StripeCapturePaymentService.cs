using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Enums;
using Delivery.Order.Domain.Contracts.RestContracts.StripeOrderUpdate;
using Delivery.Order.Domain.Contracts.V1.MessageContracts;
using Delivery.Order.Domain.Enum;
using Delivery.Order.Domain.Handlers.MessageHandlers.OrderUpdates;
using Delivery.StripePayment.Domain.Contracts.V1.MessageContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments;
using Delivery.StripePayment.Domain.Handlers.CommandHandlers.PaymentIntent.PaymentIntentConfirmation;
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
            
            // publish payment update message
            var stripeOrderUpdateContract = new StripeOrderUpdateContract
            {
                OrderId = stripePaymentCreationContract.OrderId,
                OrderStatus = OrderStatus.None,
                PaymentStatus = stripePaymentCaptureCreationStatus.PaymentStatus,
                PaymentIntentId = request.StripePaymentCaptureCreationContract.StripePaymentIntentId
            };

            var stripeOrderUpdateStatusContract = new StripeOrderUpdateStatusContract
            {
                OrderId = stripeOrderUpdateContract.OrderId,
                UpdatedDateTime = DateTimeOffset.UtcNow
            };

            await PublishStripePaymentUpdateMessageAsync(stripeOrderUpdateContract, stripeOrderUpdateStatusContract);

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

        private async Task PublishStripePaymentUpdateMessageAsync(StripeOrderUpdateContract stripeOrderUpdateContract,
            StripeOrderUpdateStatusContract stripeOrderUpdateStatusContract)
        {
            var orderUpdateMessage = new OrderUpdateMessage
            {
                PayloadIn = stripeOrderUpdateContract,
                PayloadOut = stripeOrderUpdateStatusContract,
                RequestContext = executingRequestContextAdapter.GetExecutingRequestContext()
            };

            await new OrderUpdateMessagePublisher(serviceProvider).PublishAsync(orderUpdateMessage);
            
            serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                .TrackTrace($"{nameof(PublishStripePaymentUpdateMessageAsync)} published payment update message", SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
        }
    }
}