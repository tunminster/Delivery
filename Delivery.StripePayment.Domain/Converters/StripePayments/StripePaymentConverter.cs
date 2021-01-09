using System;
using Delivery.Azure.Library.Sharding.Adapters;
using Delivery.StripePayment.Domain.Contracts.V1.RestContracts.StripePayments;

namespace Delivery.StripePayment.Domain.Converters.StripePayments
{
    public static class StripePaymentConverter
    {
        public static Database.Entities.StripePayment Convert(StripePaymentCreationContract stripePaymentCreationContract, int orderId)
        {
            var stripePayment = new Database.Entities.StripePayment
            {
                OrderId = orderId,
                StripePaymentIntentId = stripePaymentCreationContract.StripePaymentIntentId,
                StripePaymentMethodId = stripePaymentCreationContract.StripePaymentMethodId,
                PaymentStatus = stripePaymentCreationContract.PaymentStatus,
                Captured = stripePaymentCreationContract.Captured,
                AmountCaptured = stripePaymentCreationContract.AmountCaptured,
                FailureCode = stripePaymentCreationContract.FailureCode,
                FailureMessage = stripePaymentCreationContract.FailureMessage,
                CapturedDateTime = DateTimeOffset.UtcNow,
                ReceiptUrl = stripePaymentCreationContract.ReceiptUrl
            };
            
            return stripePayment;
        }
    }
}