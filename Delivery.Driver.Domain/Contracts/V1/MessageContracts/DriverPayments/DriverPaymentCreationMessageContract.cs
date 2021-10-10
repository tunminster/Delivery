using Delivery.Azure.Library.Messaging.Messages.V1;
using Delivery.Domain.Contracts.V1.RestContracts;
using Delivery.Driver.Domain.Contracts.V1.RestContracts.DriverPayments;

namespace Delivery.Driver.Domain.Contracts.V1.MessageContracts.DriverPayments
{
    public class DriverPaymentCreationMessageContract : AuditableResponseMessage<DriverPaymentCreationContract, StatusContract>
    {
    }
}