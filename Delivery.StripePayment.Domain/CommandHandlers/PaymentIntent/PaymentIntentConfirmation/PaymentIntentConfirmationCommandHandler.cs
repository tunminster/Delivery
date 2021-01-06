using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Domain.CommandHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
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
            var stripeApiKey = serviceProvider.GetRequiredService<IConfigurationProvider>().GetSetting<string>("Stripe-Api-Key");
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

            if (paymentIntentResponse.Status == "succeeded")
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace($"{nameof(StripePaymentCaptureCreationContract)} payment status is succeeded.", 
                        SeverityLevel.Information, executingRequestContextAdapter.GetTelemetryProperties());
            }
            else if (paymentIntentResponse.Status == "requires_action")
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace($"{nameof(StripePaymentCaptureCreationContract)} payment status is 'requires_action'", 
                        SeverityLevel.Warning, executingRequestContextAdapter.GetTelemetryProperties());
            }
            else
            {
                serviceProvider.GetRequiredService<IApplicationInsightsTelemetry>()
                    .TrackTrace($"{nameof(StripePaymentCaptureCreationContract)} payment status is unknown.", 
                        SeverityLevel.Error, executingRequestContextAdapter.GetTelemetryProperties());
            }

            var stripePaymentCaptureCreationStatusContract = new StripePaymentCaptureCreationStatusContract
            {
                PaymentStatus = paymentIntentResponse.Status,
                Currency = paymentIntentResponse.Currency,
                NextAction =  paymentIntentResponse.NextAction?.ToJson() ?? string.Empty,
                AmountCaptured = paymentIntentResponse.Charges.Data.FirstOrDefault()?.AmountCaptured,
                ApplicationFeeAmount = paymentIntentResponse.ApplicationFeeAmount,
                Captured = paymentIntentResponse.Charges.Data.FirstOrDefault()?.Captured ?? false,
                CaptureMethod = paymentIntentResponse.CaptureMethod,
                FailureCode = paymentIntentResponse.Charges.Data.FirstOrDefault()?.FailureCode ?? string.Empty,
                FailureMessage = paymentIntentResponse.Charges.Data.FirstOrDefault()?.FailureMessage ?? string.Empty,
                PaymentIntent = paymentIntentResponse.Charges.Data.FirstOrDefault()?.PaymentIntent.Id,
                PaymentMethod = paymentIntentResponse.Charges.Data.FirstOrDefault()?.PaymentMethod,
                ReceiptNumber = paymentIntentResponse.Charges.Data.FirstOrDefault()?.ReceiptNumber,
                ReceiptUrl = paymentIntentResponse.Charges.Data.FirstOrDefault()?.ReceiptUrl,
                LiveMode = paymentIntentResponse.Livemode
            };
            
            return stripePaymentCaptureCreationStatusContract;
        }
    }
}