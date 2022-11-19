using System;
using System.Threading.Tasks;
using Delivery.Azure.Library.Messaging.Adapters;
using Delivery.Azure.Library.Microservices.Hosting.MessageHandlers;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.Domain.Contracts.Enums;
using Delivery.Order.Domain.Contracts.V1.MessageContracts.CouponPayment;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.CouponPayments;
using Delivery.StripePayment.Domain.Handlers.CommandHandlers.CouponPayments;

namespace Delivery.StripePayment.Domain.Handlers.MessageHandlers
{
    public class CouponPaymentCreationMessageHandler : ShardedTelemeterizedMessageHandler
    {
        public CouponPaymentCreationMessageHandler(IServiceProvider serviceProvider, IExecutingRequestContextAdapter executingRequestContextAdapter) : base(serviceProvider, executingRequestContextAdapter)
        {
        }

        public async Task HandleMessageAsync(CouponPaymentCreationMessage message,
            MessageProcessingStates processingStates)
        {
            try
            {
                var messageAdapter =
                    new AuditableRequestMessageAdapter<CouponPaymentCreationMessageContract>(message);

                if (!processingStates.HasFlag(MessageProcessingStates.Processed))
                {
                    var couponPaymentCreationContract = new CouponPaymentCreationContract
                    {
                        DiscountAmount = messageAdapter.PayloadIn().DiscountAmount,
                        CouponCode = messageAdapter.PayloadIn().CouponCode,
                        OrderId = messageAdapter.PayloadIn().OrderId,
                        ShopOwnerConnectedAccount = messageAdapter.PayloadIn().ShopOwnerConnectAccount
                    };

                    var couponPaymentCommand = new CouponPaymentCommand(couponPaymentCreationContract);

                    await new CouponPaymentCommandHandler(ServiceProvider, ExecutingRequestContextAdapter)
                        .HandleAsync(couponPaymentCommand);
                    
                    processingStates |= MessageProcessingStates.Processed;
                }
            }
            catch (Exception exception)
            {
                HandleMessageProcessingFailure(processingStates, exception);
            }
        }
    }
}