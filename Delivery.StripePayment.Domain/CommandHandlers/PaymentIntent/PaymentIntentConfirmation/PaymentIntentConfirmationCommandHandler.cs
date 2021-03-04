using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Database.Factories;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.CommandHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.MessageContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments;
using Delivery.StripePayment.Domain.Handlers.MessageHandlers;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Delivery.StripePayment.Domain.CommandHandlers.PaymentIntent.PaymentIntentConfirmation
{
    public class PaymentIntentConfirmationCommandHandler :  ICommandHandler<PaymentIntentConfirmationCommand, StripePaymentCaptureCreationStatusContract>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IExecutingRequestContextAdapter executingRequestContextAdapter;

        public PaymentIntentConfirmationCommandHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter)
        {
            this.serviceProvider = serviceProvider;
            this.executingRequestContextAdapter = executingRequestContextAdapter;
        }
        
        public async Task<StripePaymentCaptureCreationStatusContract> Handle(PaymentIntentConfirmationCommand command)
        {
            var stripeApiKey = await serviceProvider.GetRequiredService<ISecretProvider>().GetSecretAsync($"Stripe-{executingRequestContextAdapter.GetShard().Key}-Api-Key");
            StripeConfiguration.ApiKey = stripeApiKey;
            
            // To create a PaymentIntent for confirmation, see our guide at: https://stripe.com/docs/payments/payment-intents/creating-payment-intents#creating-for-automatic
            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = command.StripePaymentCaptureCreationContract.StripePaymentMethodId,
            };
            
            //var requestOptions = new RequestOptions {StripeAccount = "acct_1I6NJJRLkhSmnIqS"};
            var service = new PaymentIntentService();
            var paymentIntentResponse = await service.ConfirmAsync(
                command.StripePaymentCaptureCreationContract.StripePaymentIntentId,
                options
                
            );

            var stripePaymentCaptureCreationStatusContract = new StripePaymentCaptureCreationStatusContract();

            if (paymentIntentResponse.Status == "succeeded")
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace($"{nameof(StripePaymentCaptureCreationContract)} payment status is succeeded.", 
                        SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
                
                stripePaymentCaptureCreationStatusContract.PaymentStatus = paymentIntentResponse.Status;
                stripePaymentCaptureCreationStatusContract.PaymentResponseMessage = paymentIntentResponse.Description + paymentIntentResponse.ToJson();
                stripePaymentCaptureCreationStatusContract.Currency = paymentIntentResponse.Currency;
                stripePaymentCaptureCreationStatusContract.NextAction =
                    paymentIntentResponse.NextAction?.ToJson() ?? string.Empty;
                stripePaymentCaptureCreationStatusContract.AmountCaptured =
                    paymentIntentResponse.Charges.Data.FirstOrDefault()?.AmountCaptured;
                stripePaymentCaptureCreationStatusContract.ApplicationFeeAmount =
                    paymentIntentResponse.ApplicationFeeAmount;
                stripePaymentCaptureCreationStatusContract.Captured =
                    paymentIntentResponse.Charges.FirstOrDefault()?.Captured ?? false;
                stripePaymentCaptureCreationStatusContract.CaptureMethod = paymentIntentResponse.CaptureMethod;
                stripePaymentCaptureCreationStatusContract.FailureCode =
                    paymentIntentResponse.Charges.FirstOrDefault()?.FailureCode ?? string.Empty;
                stripePaymentCaptureCreationStatusContract.FailureMessage =
                    paymentIntentResponse.Charges.FirstOrDefault()?.FailureMessage ?? string.Empty;
                stripePaymentCaptureCreationStatusContract.PaymentIntent =
                    paymentIntentResponse.Charges.FirstOrDefault()?.PaymentIntentId ?? string.Empty;
                stripePaymentCaptureCreationStatusContract.PaymentMethod =
                    paymentIntentResponse.Charges.FirstOrDefault()?.PaymentMethod ?? string.Empty;
                stripePaymentCaptureCreationStatusContract.ReceiptNumber =
                    paymentIntentResponse.Charges.FirstOrDefault()?.ReceiptNumber ?? string.Empty;
                stripePaymentCaptureCreationStatusContract.ReceiptUrl =
                    paymentIntentResponse.Charges.FirstOrDefault()?.ReceiptUrl ?? string.Empty;
                stripePaymentCaptureCreationStatusContract.LiveMode = paymentIntentResponse.Livemode;
                
                stripePaymentCaptureCreationStatusContract.OrderId = paymentIntentResponse.Metadata.FirstOrDefault(x => x.Key == "order_id").Value ?? string.Empty;
            }
            else if (paymentIntentResponse.Status == "requires_action")
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace($"{nameof(StripePaymentCaptureCreationContract)} payment status is 'requires_action'", 
                        SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());

                stripePaymentCaptureCreationStatusContract.PaymentStatus = paymentIntentResponse.Status;
                stripePaymentCaptureCreationStatusContract.PaymentResponseMessage = paymentIntentResponse.Description;

            }
            else
            {
                stripePaymentCaptureCreationStatusContract.PaymentStatus = paymentIntentResponse.Status;
                stripePaymentCaptureCreationStatusContract.PaymentResponseMessage = paymentIntentResponse.Description;
                
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace($"{nameof(StripePaymentCaptureCreationContract)} payment status is unknown.", 
                        SeverityLevel.Error, executingRequestContextAdapter.GetTelemetryProperties());
            }

            return stripePaymentCaptureCreationStatusContract;

        }
        
        
    }
}