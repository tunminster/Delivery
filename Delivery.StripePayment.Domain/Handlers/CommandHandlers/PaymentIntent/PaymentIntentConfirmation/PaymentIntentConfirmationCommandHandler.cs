using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Azure.Library.Configuration.Configurations.Interfaces;
using Delivery.Azure.Library.Exceptions.Extensions;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Azure.Library.Telemetry.ApplicationInsights.Interfaces;
using Delivery.Database.Context;
using Delivery.Domain.CommandHandlers;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Stripe;

namespace Delivery.StripePayment.Domain.Handlers.CommandHandlers.PaymentIntent.PaymentIntentConfirmation
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
            
            await using var databaseContext = await PlatformDbContext.CreateAsync(serviceProvider, executingRequestContextAdapter);
            
            var order = await Policy.HandleResult<Delivery.Database.Entities.Order>(x => x == null)
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(value: 2))
                .ExecuteAsync(async () =>
                {
                    return await databaseContext.Orders.FirstOrDefaultAsync(x =>
                        x.PaymentIntentId == command.StripePaymentCaptureCreationContract.StripePaymentIntentId);
                });

            if (order == null)
            {
                throw new InvalidOperationException(
                        $"Order not found with payment intent id {command.StripePaymentCaptureCreationContract.StripePaymentIntentId}")
                    .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties());
            }
            
            // clone payment method to the connect account
            var clonePaymentMethodId = ClonePaymentMethodToConnectedAccount(command.StripePaymentCaptureCreationContract.StripePaymentMethodId, order.PaymentAccountNumber);
            var confirmPaymentMethodId =
                ConfirmPaymentMethod(command.StripePaymentCaptureCreationContract.StripePaymentMethodId);
            // To create a PaymentIntent for confirmation, see our guide at: https://stripe.com/docs/payments/payment-intents/creating-payment-intents#creating-for-automatic
            // var options = new PaymentIntentConfirmOptions
            // {
            //     PaymentMethod = clonePaymentMethodId
            // };
            
            // update payment method
            var paymentMethodOptions = new PaymentMethodUpdateOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", order.ExternalId },
                },
            };
            var paymentMethodService = new PaymentMethodService();
            var paymentMethodResult = await paymentMethodService.UpdateAsync(
                command.StripePaymentCaptureCreationContract.StripePaymentMethodId,
                paymentMethodOptions
            );
            
            
            
            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethodResult.Id
            };
            

            // var requestOptions = new RequestOptions
            // {
            //     StripeAccount = order.PaymentAccountNumber ?? throw new InvalidOperationException($"{command.StripePaymentCaptureCreationContract.StripePaymentIntentId} is not existed.")
            //         .WithTelemetry(executingRequestContextAdapter.GetTelemetryProperties())
            // };
            var service = new PaymentIntentService();
            // var paymentIntentResponse = await service.ConfirmAsync(
            //     command.StripePaymentCaptureCreationContract.StripePaymentIntentId,
            //     options,
            //     requestOptions
            //     
            // );
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

        private string ClonePaymentMethodToConnectedAccount(string stripePaymentMethodId, string connectedAccountId)
        {
            var options = new PaymentMethodCreateOptions
            {
                Customer = null,
                PaymentMethod = stripePaymentMethodId,
            };
            
            var requestOptions = new RequestOptions
            {
                StripeAccount = connectedAccountId,
            };
            
            var service = new PaymentMethodService();
            var paymentMethod = service.Create(options, requestOptions);
            return paymentMethod.Id;
        }
        
        private string ConfirmPaymentMethod(string stripePaymentMethodId)
        {
            var options = new PaymentMethodCreateOptions
            {
                Customer = null,
                PaymentMethod = stripePaymentMethodId,
            };
            
            
            var service = new PaymentMethodService();
            var paymentMethod = service.Create(options);
            return paymentMethod.Id;
        }
        
        
    }
}